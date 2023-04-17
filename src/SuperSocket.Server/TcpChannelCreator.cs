using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using SuperSocket.Channel;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;

namespace SuperSocket.Server
{
    public class TcpChannelCreator : IChannelCreator
    {
        private Socket _listenSocket;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<bool> _stopTaskCompletionSource;
        private readonly Func<Socket, ValueTask<IChannel>> _channelFactory;
        public ListenOptions Options { get; }
        private ILogger _logger;

        public TcpChannelCreator(ListenOptions options, Func<Socket, ValueTask<IChannel>> channelFactory, ILogger logger)
        {
            Options = options;
            _channelFactory = channelFactory;
            _logger = logger;
        }

        public bool IsRunning { get; private set; }

        public bool Start()
        {
            var options = Options;

            try
            {
                if (options.Security != SslProtocols.None && options.CertificateOptions != null)
                {
                    options.CertificateOptions.EnsureCertificate();
                }

                var listenEndpoint = options.GetListenEndPoint();
                var listenSocket = _listenSocket = new Socket(listenEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                listenSocket.LingerState = new LingerOption(false, 0);

                if (options.NoDelay)
                    listenSocket.NoDelay = true;
                
                listenSocket.Bind(listenEndpoint);
                listenSocket.Listen(options.BackLog);

                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();

                KeepAccept(listenSocket).DoNotAwait();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"The listener[{this.ToString()}] failed to start.");
                return false;
            }
        }

        private async Task KeepAccept(Socket listenSocket)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var client = await listenSocket.AcceptAsync().ConfigureAwait(false);
                    OnNewClientAccept(client);
                }
                catch (Exception e)
                {
                    if (e is ObjectDisposedException || e is NullReferenceException)
                        break;
                    
                    if (e is SocketException se)
                    {
                        var errorCode = se.ErrorCode;

                        //The listen socket was closed
                        if (errorCode == 125 || errorCode == 89 || errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                        {
                            break;
                        }
                    }
                    
                    _logger.LogError(e, $"Listener[{this.ToString()}] failed to do AcceptAsync");
                    continue;
                }
            }

            _stopTaskCompletionSource.TrySetResult(true);
        }

        public event NewClientAcceptHandler NewClientAccepted;

        private async void OnNewClientAccept(Socket socket)
        {
            var handler = NewClientAccepted;

            if (handler == null)
                return;

            IChannel channel = null;

            try
            {
                channel = await _channelFactory(socket);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to create channel for {socket.RemoteEndPoint}.");
                return;
            }            

            await handler.Invoke(this, channel);
        }

        public async Task<IChannel> CreateChannel(object connection)
        {
            return await _channelFactory((Socket)connection);
        }

        public Task StopAsync()
        {
            var listenSocket = _listenSocket;

            if (listenSocket == null)
                return Task.Delay(0);

            _stopTaskCompletionSource = new TaskCompletionSource<bool>();

            _cancellationTokenSource.Cancel();
            listenSocket.Close();
            
            return _stopTaskCompletionSource.Task;
        }

        public override string ToString()
        {
            return Options?.ToString();
        }
    }
}