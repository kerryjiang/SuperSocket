using System;
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

namespace SuperSocket.Tests
{
    public class ProxyProtocolHostConfigurator : IHostConfigurator
    {
        private IHostConfigurator _innerHostConfigurator;

        private static readonly byte[] _proxyProtocolV2_IPV4_SampleData = new byte[]
            {
                0x0D, 0x0A, 0x0D, 0x0A,
                0x00, 0x0D, 0x0A, 0x51,
                0x55, 0x49, 0x54, 0x0A,
                0x21, 0x11, 0x00, 0x0c,
                0xac, 0x13, 0x00, 0x01,
                0xac, 0x13, 0x00, 0x03,
                0xa6, 0x52, 0x00, 0x50
            };

        public ProxyProtocolHostConfigurator(IHostConfigurator hostConfigurator)
        {
            _innerHostConfigurator = hostConfigurator;
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
            
            await stream.WriteAsync(_proxyProtocolV2_IPV4_SampleData, 0, _proxyProtocolV2_IPV4_SampleData.Length);
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
    }
}