using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using SuperSocket.Channel;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Server
{
    public class TcpChannelCreator : IChannelCreator
    {
        private Socket _listenSocket;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<bool> _stopTaskCompletionSource;
        private readonly Func<Socket, Task<IChannel>> _channelFactory;
        public ListenOptions Options { get; }
        private ILogger _logger;

        public TcpChannelCreator(ListenOptions options, Func<Socket, Task<IChannel>> channelFactory, ILogger logger)
        {
            Options = options;
            _channelFactory = channelFactory;
            _logger = logger;
        }

        private IPEndPoint GetListenEndPoint(string ip, int port)
        {
            IPAddress ipAddress;

            if ("any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.Any;
            }
            else if ("IpV6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.IPv6Any;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }

            return new IPEndPoint(ipAddress, port);
        }

        public bool IsRunning { get; private set; }

        public bool Start()
        {
            var options = Options;

            try
            {
                var listenEndpoint = GetListenEndPoint(options.Ip, options.Port);
                var listenSocket = _listenSocket = new Socket(listenEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                listenSocket.LingerState = new LingerOption(false, 0);

                if (options.NoDelay)
                    listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                
                listenSocket.Bind(listenEndpoint);
                listenSocket.Listen(options.BackLog);

                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();

                KeepAccept(listenSocket).DoNotAwait();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "The listener failed to start.");
                return false;
            }
        }

        private async Task KeepAccept(Socket listenSocket)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var client = await listenSocket.AcceptAsync();
                    await OnNewClientAccept(client);
                }
                catch (Exception)
                {
                    break;
                }
            }

            _stopTaskCompletionSource.TrySetResult(true);
        }

        public event NewClientAcceptHandler NewClientAccepted;

        private async Task OnNewClientAccept(Socket socket)
        {
            var handler = NewClientAccepted;

            handler?.Invoke(this, await _channelFactory(socket));
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