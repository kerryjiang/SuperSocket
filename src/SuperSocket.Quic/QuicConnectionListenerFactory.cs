using System;
using System.Collections.Generic;
using System.Net.Quic;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Quic;
#pragma warning disable CA2252
internal class QuicConnectionListenerFactory : IConnectionListenerFactory
{
    private readonly QuicTransportOptions _quicTransportOptions;
    private readonly IConnectionFactoryBuilder _connectionFactoryBuilder;

    public QuicConnectionListenerFactory(IConnectionFactoryBuilder connectionFactoryBuilder,
        IOptions<QuicTransportOptions> options)
    {
        _connectionFactoryBuilder = connectionFactoryBuilder;
        _quicTransportOptions = options.Value;
    }

    public IConnectionListener CreateConnectionListener(ListenOptions options, ConnectionOptions connectionOptions,
        ILoggerFactory loggerFactory)
    {
        connectionOptions.Logger = loggerFactory.CreateLogger(nameof(IConnection));
        var connectionFactoryLogger = loggerFactory.CreateLogger(nameof(QuicConnectionListener));

        var connectionFactory = _connectionFactoryBuilder.Build(options, connectionOptions);

        var listenEndpoint = options.ToEndPoint();

        if (options.CertificateOptions == null)
            throw new ArgumentNullException(nameof(options.CertificateOptions),"Quic requires an ssl certificate");
            
        if (options.CertificateOptions.Certificate == null)
            options.CertificateOptions.EnsureCertificate();

        var quicListenerOptions = new QuicListenerOptions
        {
            ListenBacklog = options.BackLog,
            ListenEndPoint = listenEndpoint,
            ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http3 },
            ConnectionOptionsCallback = (connection, ssl, token) => ValueTask.FromResult(
                new QuicServerConnectionOptions
                {
                    DefaultStreamErrorCode = _quicTransportOptions.DefaultStreamErrorCode,
                    DefaultCloseErrorCode = _quicTransportOptions.DefaultCloseErrorCode,
                    IdleTimeout = _quicTransportOptions.IdleTimeout.HasValue
                        ? TimeSpan.FromMicroseconds(_quicTransportOptions.IdleTimeout.Value)
                        : Timeout.InfiniteTimeSpan,
                    MaxInboundBidirectionalStreams = _quicTransportOptions.MaxBidirectionalStreamCount,
                    MaxInboundUnidirectionalStreams = _quicTransportOptions.MaxUnidirectionalStreamCount,
                    ServerAuthenticationOptions = new SslServerAuthenticationOptions()
                    {
                        ApplicationProtocols =
                            new List<SslApplicationProtocol> { SslApplicationProtocol.Http3 },
                        ServerCertificate = options.CertificateOptions.Certificate,
                        RemoteCertificateValidationCallback =
                            options.CertificateOptions.RemoteCertificateValidationCallback,
                    }
                })
        };
        
        return new QuicConnectionListener(options, _quicTransportOptions, connectionFactory,
            connectionFactoryLogger);
    }
}