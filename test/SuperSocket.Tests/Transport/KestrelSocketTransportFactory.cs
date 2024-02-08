using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;

namespace SuperSocket.Tests
{
    internal sealed class KestrelSocketTransportFactory
        : IChannelCreator
    {
        private readonly ILogger _logger;
        private readonly IConnectionListenerFactory _listenerFactory;
        private readonly Func<ConnectionContext, ValueTask<IChannel>> _channelFactory;
        private IConnectionListener _connectionListener;
        private CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<bool> _stopTaskCompletionSource;

        public ListenOptions Options { get; }

        public event NewClientAcceptHandler NewClientAccepted;

        public bool IsRunning { get; private set; }

        Task<IChannel> IChannelCreator.CreateChannel(object connection) => throw new NotImplementedException();

        public KestrelSocketTransportFactory(ListenOptions options,
            IConnectionListenerFactory socketTransportFactory,
            Func<ConnectionContext, ValueTask<IChannel>> channelFactory,
            ILogger logger)
        {
            Options = options;
            _logger = logger;
            _listenerFactory = socketTransportFactory;
            _channelFactory = channelFactory;
        }

        bool IChannelCreator.Start()
        {
            try
            {
                var listenEndpoint = Options.GetListenEndPoint();

                var result = _listenerFactory.BindAsync(listenEndpoint);

                _connectionListener = result.IsCompleted ? result.Result : result.GetAwaiter().GetResult();

                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();

                KeepAcceptAsync(_connectionListener).DoNotAwait();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"The listener[{this.ToString()}] failed to start.");
                return false;
            }
        }

        async Task IChannelCreator.StopAsync()
        {
            var listenSocket = _connectionListener;

            if (listenSocket == null)
                return;

            _stopTaskCompletionSource = new TaskCompletionSource<bool>();

            _cancellationTokenSource.Cancel();

            await _connectionListener.UnbindAsync();

            await _stopTaskCompletionSource.Task;
        }

        private async Task KeepAcceptAsync(IConnectionListener connectionListener)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var client = await connectionListener.AcceptAsync().ConfigureAwait(false);

                    if (client != null)
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
                        if (errorCode == 125 || errorCode == 89 || errorCode == 995 || errorCode == 10004 ||
                            errorCode == 10038)
                        {
                            break;
                        }
                    }

                    _logger.LogError(e, $"Listener[{this.ToString()}] failed to do AcceptAsync");
                }
            }

            _stopTaskCompletionSource.TrySetResult(true);
        }

        private async void OnNewClientAccept(ConnectionContext context)
        {
            var handler = NewClientAccepted;

            if (handler == null)
                return;

            IChannel channel = null;

            try
            {
                channel = await _channelFactory(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to create channel for {context.RemoteEndPoint}.");
                return;
            }

            await handler.Invoke(this, channel);
        }

        public override string ToString()
        {
            return Options?.ToString();
        }
    }
}