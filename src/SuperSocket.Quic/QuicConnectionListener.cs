using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using System.Net.Quic;
using System.Net.Security;

#pragma warning disable CA2252
namespace SuperSocket.Quic
{
    internal sealed class QuicConnectionListener : IConnectionListener
    {
        private readonly ILogger _logger;

        private QuicListener _listenSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<bool> _stopTaskCompletionSource;
        public IConnectionFactory ConnectionFactory { get; }
        public ListenOptions Options { get; }
        public bool IsRunning { get; private set; }

        public QuicConnectionListener(ListenOptions options, IConnectionFactory connectionFactory, ILogger logger)
        {
            Options = options;
            ConnectionFactory = connectionFactory;
            _logger = logger;
        }

        public bool Start()
        {
            var options = Options;

            try
            {
                var listenEndpoint = options.ToEndPoint();

                ArgumentNullException.ThrowIfNull(options.CertificateOptions);
                
                if (options.CertificateOptions.Certificate == null)
                    options.CertificateOptions.EnsureCertificate();
                
                var quicListenerOptions = new QuicListenerOptions
                {
                    ListenBacklog = options.BackLog,
                    ListenEndPoint = listenEndpoint,
                    ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http3 },
                    ConnectionOptionsCallback = (connection, ssl, token) => ValueTask.FromResult(
                        new QuicServerConnectionOptions()
                        {
                            DefaultStreamErrorCode = 0,
                            DefaultCloseErrorCode = 0,
                            //IdleTimeout = 0,
                            //MaxInboundBidirectionalStreams = 0,
                            //MaxInboundUnidirectionalStreams = 0,
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

                var result = QuicListener.ListenAsync(quicListenerOptions);

                if (result.IsCompleted)
                    _listenSocket = result.Result;
                else
                    _listenSocket = result.GetAwaiter().GetResult();

                var listenSocket = _listenSocket;

                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();

                KeepAcceptAsync(listenSocket, _cancellationTokenSource.Token).DoNotAwait();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"The listener[{this.ToString()}] failed to start.");
                return false;
            }
        }


        private async Task KeepAcceptAsync(QuicListener listenSocket, CancellationToken cancellationToken)

        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var quicConnection =
                        await listenSocket.AcceptConnectionAsync(cancellationToken).ConfigureAwait(false);
                    OnNewClientAccept(quicConnection, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Listener[{this.ToString()}] failed to do AcceptAsync");
                }
            }

            _stopTaskCompletionSource.TrySetResult(true);
        }

        public event NewConnectionAcceptHandler NewConnectionAccept;

        private async void OnNewClientAccept(QuicConnection quicConnection, CancellationToken cancellationToken)
        {
            var handler = NewConnectionAccept;

            if (handler == null)
                return;

            IConnection connection = null;

            try
            {
                using var cts = CancellationTokenSourcePool.Shared.Rent(Options.ConnectionAcceptTimeOut);
                connection = await ConnectionFactory.CreateConnection(quicConnection, cts.Token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to create quicConnection for {quicConnection.RemoteEndPoint}.");
                return;
            }

            await handler.Invoke(this.Options, connection);
        }

        public async Task StopAsync()
        {
            var listenSocket = _listenSocket;

            if (listenSocket == null)
                return;

            _stopTaskCompletionSource = new TaskCompletionSource<bool>();

            _cancellationTokenSource.Cancel();
            await _listenSocket.DisposeAsync();

            await _stopTaskCompletionSource.Task;
        }

        public override string ToString()
        {
            return Options?.ToString();
        }
    }
}
#pragma warning restore CA2252