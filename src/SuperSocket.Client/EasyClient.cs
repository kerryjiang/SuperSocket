using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Client
{
    public abstract class EasyClient<TPackage> : EasyClient<TPackage, TPackage>
        where TPackage : class
    {

    }

    public abstract class EasyClient<TReceivePackage, TSendPackage>
        where TReceivePackage : class
        where TSendPackage : class
    {
        private IPipelineFilter<TReceivePackage> _pipelineFilter;

        private IChannel<TReceivePackage> _channel;

        private ILogger _logger;

        private Action<TReceivePackage> _handler;

        private IPackageEncoder<TSendPackage> _packageEncoder;

        public void Initialize(IPipelineFilter<TReceivePackage> pipelineFilter, Action<TReceivePackage> handler, IPackageEncoder<TSendPackage> packageEncoder = null, ILogger logger = null)
        {
            _pipelineFilter = pipelineFilter;
            _handler = handler;
            _packageEncoder = packageEncoder;
            _logger = logger;
        }

        public async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint)
        {
            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(remoteEndPoint);
                _channel = new TcpPipeChannel<TReceivePackage>(socket, _pipelineFilter, _logger);
                _channel.PackageReceived += (c, p) => 
                {
                    _handler(p);
                };
                _channel.Closed += (s, e) =>
                {
                    Closed?.Invoke(s, e);
                };
                return true;
            }
            catch (Exception e)
            {
                OnError($"Failed to connect to {remoteEndPoint}", e);
                return false;
            }
        }

        protected void OnError(string message, Exception exception)
        {
            _logger?.LogError(exception, message);
        }

        public virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            await _channel.SendAsync(data);
        }

        public virtual async ValueTask SendAsync(TSendPackage package)
        {
            await _channel.SendAsync(_packageEncoder, package);
        }

        public event EventHandler Closed;

        public void Close()
        {
            _channel?.Close();
        }
    }
}
