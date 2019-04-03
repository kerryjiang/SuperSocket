using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Client
{
    public class EasyClient<TPackage> : EasyClient<TPackage, TPackage>
        where TPackage : class
    {
        public EasyClient(IPipelineFilter<TPackage> pipelineFilter, Func<TPackage, Task> handler, IPackageEncoder<TPackage> packageEncoder = null, ILogger logger = null)
            : base(pipelineFilter, handler, packageEncoder, logger)
        {

        }
    }

    public class EasyClient<TReceivePackage, TSendPackage>
        where TReceivePackage : class
        where TSendPackage : class
    {
        private IPipelineFilter<TReceivePackage> _pipelineFilter;

        private IChannel<TReceivePackage> _channel;

        private ILogger _logger;

        private Func<TReceivePackage, Task> _handler;

        private IPackageEncoder<TSendPackage> _packageEncoder;

        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, Func<TReceivePackage, Task> handler, IPackageEncoder<TSendPackage> packageEncoder = null, ILogger logger = null)
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
                _channel.PackageReceived += OnPackageReceived;
                _channel.Closed += OnClosed;
                return true;
            }
            catch (Exception e)
            {
                OnError($"Failed to connect to {remoteEndPoint}", e);
                return false;
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _channel.PackageReceived -= OnPackageReceived;
            _channel.Closed -= OnClosed;
            Closed?.Invoke(sender, e);
        }

        protected async Task OnPackageReceived(IChannel channel, TReceivePackage package)
        {
            await _handler?.Invoke(package);
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
