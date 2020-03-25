using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace SuperSocket.Client
{
    public class EasyClient<TPackage, TSendPackage> : EasyClient<TPackage>, IEasyClient<TPackage, TSendPackage>
        where TPackage : class
    {
        private IPackageEncoder<TSendPackage> _packageEncoder;
        
        public EasyClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder = null, ILogger logger = null)
            : base(pipelineFilter, logger)
        {
            _packageEncoder = packageEncoder;
        }

        public virtual async ValueTask SendAsync(TSendPackage package)
        {
            await SendAsync(_packageEncoder, package);
        }

        public new IEasyClient<TPackage, TSendPackage> AsClient()
        {
            return this;
        }
    }

    public class EasyClient<TReceivePackage> : IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        private IPipelineFilter<TReceivePackage> _pipelineFilter;

        private IChannel<TReceivePackage> _channel;

        private ILogger _logger;

        IAsyncEnumerator<TReceivePackage> _packageEnumerator;

        public event PackageHandler<TReceivePackage> PackageHandler;

        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger = null)
        {
            _pipelineFilter = pipelineFilter;
            _logger = logger;
        }

        public virtual IEasyClient<TReceivePackage> AsClient()
        {
            return this;
        }

        protected virtual IConnector GetConntector()
        {
            return new SocketConnector();
        }

        protected virtual ChannelOptions GetChannelOptions()
        {
            return new ChannelOptions
                {
                    Logger = _logger
                };
        }

        ValueTask<bool> IEasyClient<TReceivePackage>.ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return ConnectAsync(remoteEndPoint, cancellationToken);
        }

        protected virtual async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            var connector = GetConntector();
            var state = await connector.ConnectAsync(remoteEndPoint, null, cancellationToken);

            if (state.Cancelled || cancellationToken.IsCancellationRequested)
            {
                OnError($"The connection to {remoteEndPoint} was cancelled.", state.Exception);
                return false;
            }                

            if (!state.Result)
            {
                OnError($"Failed to connect to {remoteEndPoint}", state.Exception);
                return false;
            }

            var socket = state.Socket;

            if (socket == null)
                throw new Exception("Socket is null.");

            var channelOptions = GetChannelOptions();

            if (state.Stream != null)
            {
                _channel = new StreamPipeChannel<TReceivePackage>(state.Stream , socket.RemoteEndPoint, socket.LocalEndPoint, _pipelineFilter, channelOptions);
            }
            else
            {
                _channel = new TcpPipeChannel<TReceivePackage>(socket, _pipelineFilter, channelOptions);
            }

            _channel.Closed += OnChannelClosed;
            _packageEnumerator = _channel.RunAsync().GetAsyncEnumerator();
            return true;
        }

        ValueTask<TReceivePackage> IEasyClient<TReceivePackage>.ReceiveAsync()
        {
            return ReceiveAsync();
        }

        /// <summary>
        /// Try to receive one package
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask<TReceivePackage> ReceiveAsync()
        {
            var enumerator = _packageEnumerator;

            if (await enumerator.MoveNextAsync())
                return enumerator.Current;

            OnClosed(_channel, EventArgs.Empty);
            return null;
        }

        void IEasyClient<TReceivePackage>.StartReceive()
        {
            StartReceive();
        }

        /// <summary>
        /// Start receive packages and handle the packages by event handler
        /// </summary>
        protected virtual void StartReceive()
        {
            StartReceiveAsync();
        }

        private async void StartReceiveAsync()
        {
            var enumerator = _packageEnumerator;

            while (await enumerator.MoveNextAsync())
            {
                OnPackageReceived(enumerator.Current);
            }
        }

        private void OnPackageReceived(TReceivePackage package)
        {
            var handler = PackageHandler;

            try
            {
                handler.Invoke(this, package);
            }
            catch (Exception e)
            {
                OnError("Unhandled exception happened in PackageHandler.", e);
            }
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            _channel.Closed -= OnChannelClosed;
            OnClosed(this, e);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            var handler = Closed;

            if (handler != null)
            {
                if (Interlocked.CompareExchange(ref Closed, null, handler) == handler)
                {
                    handler.Invoke(sender, e);
                }
            }
        }

        protected virtual void OnError(string message, Exception exception)
        {
            _logger?.LogError(exception, message);
        }

        ValueTask IEasyClient<TReceivePackage>.SendAsync(ReadOnlyMemory<byte> data)
        {
            return SendAsync(data);
        }

        protected virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            await _channel.SendAsync(data);
        }

        ValueTask IEasyClient<TReceivePackage>.SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            return SendAsync<TSendPackage>(packageEncoder, package);
        }

        protected virtual async ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            await _channel.SendAsync(packageEncoder, package);
        }

        public event EventHandler Closed;

        public async ValueTask CloseAsync()
        {
            await _channel.CloseAsync();
            OnClosed(this, EventArgs.Empty);
        }
    }
}
