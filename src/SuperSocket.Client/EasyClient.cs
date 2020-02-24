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
    public class EasyClient<TPackage, TSendPackage> : EasyClient<TPackage>
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
    }

    public class EasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        private IPipelineFilter<TReceivePackage> _pipelineFilter;

        private IChannel<TReceivePackage> _channel;

        private ILogger _logger;

        IAsyncEnumerator<TReceivePackage> _packageEnumerator;

        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger = null)
        {
            _pipelineFilter = pipelineFilter;
            _logger = logger;
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

        public async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint)
        {
            return await ConnectAsync(remoteEndPoint, CancellationToken.None);
        }

        public virtual async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
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

            _packageEnumerator = _channel.RunAsync().GetAsyncEnumerator();                
            return true;
        }

        public async ValueTask<TReceivePackage> ReceiveAsync()
        {
            var enumerator = _packageEnumerator;

            if (await enumerator.MoveNextAsync())
                return enumerator.Current;

            OnClosed(_channel, EventArgs.Empty);
            return null;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(sender, e);
        }

        protected void OnError(string message, Exception exception)
        {
            _logger?.LogError(exception, message);
        }

        public virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            await _channel.SendAsync(data);
        }

        public virtual async ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            await _channel.SendAsync(packageEncoder, package);
        }

        public event EventHandler Closed;

        public void Close()
        {
            _channel?.Close();
        }
    }
}
