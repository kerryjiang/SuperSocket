using System;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.Channel.Kestrel;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.Tests
{
    internal sealed class KestrelSocketChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
    {
        private static SocketConnectionContextFactory _socketConnectionContextFactory;

        public KestrelSocketChannelCreatorFactory(ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider) : base(serviceProvider)

        {
            var logger = loggerFactory.CreateLogger<SocketConnectionContextFactory>();
            _socketConnectionContextFactory ??=
                new SocketConnectionContextFactory(new SocketConnectionFactoryOptions(), logger);
        }

        IChannelCreator IChannelCreatorFactory.CreateChannelCreator<TPackageInfo>(
            ListenOptions options,
            ChannelOptions channelOptions,
            ILoggerFactory loggerFactory,
            object pipelineFilterFactory)
        {
            var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;

            ArgumentNullException.ThrowIfNull(filterFactory);

            var channelFactoryLogger = loggerFactory.CreateLogger(nameof(KestrelSocketTransportFactory));
            channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

            if (options.Security == SslProtocols.None)
            {
                return new TcpChannelCreator(
                    options: options,
                    logger: channelFactoryLogger, channelFactory:
                    socket =>
                    {
                        ApplySocketOptions(socket, options, channelOptions, channelFactoryLogger);

                        var connectionContext = _socketConnectionContextFactory.Create(socket);

                        var filter = filterFactory.Create(connectionContext);

                        var channel = new TransportPipeChannel<TPackageInfo>(
                            connectionContext.Transport,
                            connectionContext.LocalEndPoint,
                            connectionContext.RemoteEndPoint,
                            filter,
                            channelOptions);

                        return new ValueTask<IChannel>(channel);
                    });
            }
            else
            {
                var channelFactory = new Func<Socket, ValueTask<IChannel>>(async (s) =>
                {
                    ApplySocketOptions(s, options, channelOptions, channelFactoryLogger);

                    var authOptions = new SslServerAuthenticationOptions
                    {
                        EnabledSslProtocols = options.Security,
                        ServerCertificate = options.CertificateOptions.Certificate,
                        ClientCertificateRequired = options.CertificateOptions.ClientCertificateRequired
                    };

                    if (options.CertificateOptions.RemoteCertificateValidationCallback != null)
                        authOptions.RemoteCertificateValidationCallback =
                            options.CertificateOptions.RemoteCertificateValidationCallback;

                    var stream = new SslStream(new NetworkStream(s, true), false);
                    await stream.AuthenticateAsServerAsync(authOptions, CancellationToken.None).ConfigureAwait(false);
                    return new SslStreamPipeChannel<TPackageInfo>(stream, s.RemoteEndPoint, s.LocalEndPoint,
                        filterFactory.Create(s), channelOptions);
                });

                return new TcpChannelCreator(options, channelFactory, channelFactoryLogger);
            }
        }
    }

    internal sealed class TransportChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
    {
        public TransportChannelCreatorFactory(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public new IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options,
            ChannelOptions channelOptions,
            ILoggerFactory loggerFactory, object pipelineFilterFactory)
        {
            var filterFactory = (IPipelineFilterFactory<TPackageInfo>)pipelineFilterFactory;

            channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

            var channelFactoryLogger = loggerFactory.CreateLogger(nameof(TcpChannelCreator));

            if (options.Security == SslProtocols.None)
            {
                return new TcpChannelCreator(options, (socket) =>
                {
                    ApplySocketOptions(socket, options, channelOptions, channelFactoryLogger);

                    var stream = new NetworkStream(socket, true);
                    var reader = PipeReader.Create(stream);
                    var writer = PipeWriter.Create(stream);

                    IChannel channel =
                        new TransportPipeChannel<TPackageInfo>(reader, writer, socket.LocalEndPoint,
                            socket.RemoteEndPoint,
                            filterFactory.Create(socket), channelOptions);

                    return new ValueTask<IChannel>(channel);
                }, channelFactoryLogger);
            }
            else
            {
                var channelFactory = new Func<Socket, ValueTask<IChannel>>(async (socket) =>
                {
                    ApplySocketOptions(socket, options, channelOptions, channelFactoryLogger);

                    var authOptions = new SslServerAuthenticationOptions
                    {
                        EnabledSslProtocols = options.Security,
                        ServerCertificate = options.CertificateOptions.Certificate,
                        ClientCertificateRequired = options.CertificateOptions.ClientCertificateRequired
                    };

                    if (options.CertificateOptions.RemoteCertificateValidationCallback != null)
                        authOptions.RemoteCertificateValidationCallback =
                            options.CertificateOptions.RemoteCertificateValidationCallback;

                    var stream = new SslStream(new NetworkStream(socket, true), false);
                    await stream.AuthenticateAsServerAsync(authOptions, CancellationToken.None).ConfigureAwait(false);

                    var reader = PipeReader.Create(stream);
                    var writer = PipeWriter.Create(stream);

                    return new TransportPipeChannel<TPackageInfo>(reader, writer, socket.LocalEndPoint,
                        socket.RemoteEndPoint,
                        filterFactory.Create(socket), channelOptions);
                });

                return new TcpChannelCreator(options, channelFactory, channelFactoryLogger);
            }
        }
    }
}