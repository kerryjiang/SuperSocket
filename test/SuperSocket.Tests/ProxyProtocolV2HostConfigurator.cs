using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace SuperSocket.Tests
{
    public class ProxyProtocolV2HostConfigurator : ProxyProtocolHostConfigurator
    {
        private static readonly byte[] _proxyProtocolV2_SIGNATURE = new byte[]
            {
                // Signature
                0x0D, 0x0A, 0x0D, 0x0A,
                0x00, 0x0D, 0x0A, 0x51,
                0x55, 0x49, 0x54, 0x0A
            };

        protected override byte[] CreateProxyProtocolData(IPEndPoint sourceIPEndPoint, IPEndPoint destinationIPEndPoint)
        {
            var isIpV4 = sourceIPEndPoint.Address.AddressFamily == AddressFamily.InterNetwork;
            var ipAddressLength = isIpV4 ? 4 : 16;

            var addressLength = isIpV4
                ? (ipAddressLength * 2 + 4)
                : (ipAddressLength * 2 + 4);

            var data = new byte[_proxyProtocolV2_SIGNATURE.Length  + 4 + addressLength];

            _proxyProtocolV2_SIGNATURE.CopyTo(data, 0);

            data[_proxyProtocolV2_SIGNATURE.Length] = 0x21;
            data[_proxyProtocolV2_SIGNATURE.Length + 1] = (byte)((InnerHostConfigurator is UdpHostConfigurator ? 0x02 : 0x01) | (isIpV4 ? 0x10 : 0x20));

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

        public ProxyProtocolV2HostConfigurator(IHostConfigurator hostConfigurator, IPEndPoint sourceIPEndPoint, IPEndPoint destinationIPEndPoint)
            : base(hostConfigurator, sourceIPEndPoint, destinationIPEndPoint)
        {
        }
    }
}