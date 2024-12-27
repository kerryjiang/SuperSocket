using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Client;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;
using Xunit;

namespace SuperSocket.Tests
{
    public abstract class ProxyProtocolHostConfigurator : IHostConfigurator
    {
        private IHostConfigurator _innerHostConfigurator;

        protected IHostConfigurator InnerHostConfigurator
        {
            get { return _innerHostConfigurator; }
        }

        private IPEndPoint _sourceIPEndPoint;
        private IPEndPoint _destinationIPEndPoint;

        protected abstract byte[] CreateProxyProtocolData(IPEndPoint sourceIPEndPoint, IPEndPoint destinationIPEndPoint);

        public ProxyProtocolHostConfigurator(IHostConfigurator hostConfigurator, IPEndPoint sourceIPEndPoint, IPEndPoint destinationIPEndPoint)
        {
            _innerHostConfigurator = hostConfigurator;
            _sourceIPEndPoint = sourceIPEndPoint;
            _destinationIPEndPoint = destinationIPEndPoint;
        }

        public string WebSocketSchema => _innerHostConfigurator.WebSocketSchema;

        public bool IsSecure => _innerHostConfigurator.IsSecure;

        public ListenOptions Listener => _innerHostConfigurator.Listener;

        public void Configure(ISuperSocketHostBuilder hostBuilder)
        {
            _innerHostConfigurator.Configure(hostBuilder);
        }

        public IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options) where TPackageInfo : class
        {
            return _innerHostConfigurator.ConfigureEasyClient<TPackageInfo>(pipelineFilter, options);
        }

        public Socket CreateClient()
        {
            return _innerHostConfigurator.CreateClient();
        }

        public async ValueTask<Stream> GetClientStream(Socket socket)
        {
            var stream = await _innerHostConfigurator.GetClientStream(socket);

            stream.Write(CreateProxyProtocolData(_sourceIPEndPoint, _destinationIPEndPoint));
            await stream.FlushAsync();

            return stream;
        }

        public TextReader GetStreamReader(Stream stream, Encoding encoding)
        {
            return _innerHostConfigurator.GetStreamReader(stream, encoding);
        }

        public ValueTask KeepSequence()
        {
            return _innerHostConfigurator.KeepSequence();
        }

        public ValueTask<Socket> CreateConnectedClientAsync()
        {
            return _innerHostConfigurator.CreateConnectedClientAsync();
        }
    }
}