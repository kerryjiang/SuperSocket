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
    public class ProxyProtocolHostConfigurator : IHostConfigurator
    {
        private IHostConfigurator _innerHostConfigurator;

        private static readonly byte[] _proxyProtocolV2_SIGNATURE = new byte[]
            {
                // Signature
                0x0D, 0x0A, 0x0D, 0x0A,
                0x00, 0x0D, 0x0A, 0x51,
                0x55, 0x49, 0x54, 0x0A
            };

        private IPEndPoint _sourceIPEndPoint;
        private IPEndPoint _destinationIPEndPoint;

        private byte[] CreateProxyProtocolData(IPEndPoint sourceIPEndPoint, IPEndPoint destinationIPEndPoint)
        {
            var isIpV4 = sourceIPEndPoint.Address.AddressFamily == AddressFamily.InterNetwork;
            var ipAddressLength = isIpV4 ? 4 : 16;

            var addressLength = isIpV4
                ? (ipAddressLength * 2 + 4)
                : (ipAddressLength * 2 + 4);

            var data = new byte[_proxyProtocolV2_SIGNATURE.Length  + 4 + addressLength];

            _proxyProtocolV2_SIGNATURE.CopyTo(data, 0);

            data[_proxyProtocolV2_SIGNATURE.Length] = 0x21;
            data[_proxyProtocolV2_SIGNATURE.Length + 1] = (byte)((_innerHostConfigurator is UdpHostConfigurator ? 0x02 : 0x01) | (isIpV4 ? 0x10 : 0x20));

            var span = data.AsSpan().Slice(_proxyProtocolV2_SIGNATURE.Length);

            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2, 2), (ushort)addressLength);

            var spanToWrite = span.Slice(4);

            var addressSpan = spanToWrite.Slice(0, ipAddressLength);

            var written = 0;

            sourceIPEndPoint.Address.TryWriteBytes(addressSpan, out written);

            Assert.Equal(ipAddressLength, written);            

            spanToWrite = spanToWrite.Slice(ipAddressLength);

            addressSpan = spanToWrite.Slice(0, ipAddressLength);
            destinationIPEndPoint.Address.TryWriteBytes(addressSpan, out written);

            Assert.Equal(ipAddressLength, written);

            spanToWrite = spanToWrite.Slice(ipAddressLength);

            BinaryPrimitives.WriteUInt16BigEndian(spanToWrite.Slice(0, 2), (ushort)sourceIPEndPoint.Port);
            BinaryPrimitives.WriteUInt16BigEndian(spanToWrite.Slice(2, 2), (ushort)destinationIPEndPoint.Port);
  
            return data;
        }

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
    }
}