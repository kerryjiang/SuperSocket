using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    class ProxyProtocolV2PartReader<TPackageInfo> : ProxyProtocolPackagePartReader<TPackageInfo>
    {
        private const int FIXPART_LEN_AFTER_SIGNATURE = 4;

        private const int IPV6_ADDRESS_LEN = 16;

        private const int IPV6_ADDRESS_ALL_LEN = 16 * 2;

        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        public override bool Process(TPackageInfo package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData)
        {
            nextPartReader = null;

            var proxyInfo = filterContext as ProxyInfo;

            if (proxyInfo.AddressLength == 0)
            {
                if (reader.Length < FIXPART_LEN_AFTER_SIGNATURE)
                {
                    needMoreData = true;
                    return false;
                }

                reader.TryRead(out var versionAndCommand);

                proxyInfo.Version = (int)versionAndCommand / 16;
                proxyInfo.Command = (versionAndCommand % 16) == 0 ? ProxyCommand.LOCAL : ProxyCommand.PROXY;

                reader.TryRead(out var addressFamilyAndProtocol);

                proxyInfo.AddressFamily = ((int)addressFamilyAndProtocol / 16) switch            
                {
                    0 => AddressFamily.Unspecified,
                    1 => AddressFamily.InterNetwork,
                    2 => AddressFamily.InterNetworkV6,
                    3 => AddressFamily.Unix,
                    _ => throw new NotSupportedException(),
                };

                proxyInfo.ProtocolType = ((int)addressFamilyAndProtocol % 16) switch
                {
                    0 => ProtocolType.Unspecified,
                    1 => ProtocolType.Tcp,
                    2 => ProtocolType.Udp,
                    _ => throw new NotSupportedException(),
                };

                reader.TryRead(out var len_high);
                reader.TryRead(out var len_low);

                proxyInfo.AddressLength = len_high * 256 + len_low;

                needMoreData = false;
                return false;
            }

            if (reader.Length < proxyInfo.AddressLength)
            {
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            if (proxyInfo.AddressFamily == AddressFamily.InterNetwork)
            {
                reader.TryReadBigEndian(out UInt32 sourceIpData);

                if (BitConverter.IsLittleEndian)
                    sourceIpData = BinaryPrimitives.ReverseEndianness(sourceIpData);

                proxyInfo.SourceIPAddress = new IPAddress(sourceIpData);

                reader.TryReadBigEndian(out UInt32 destinationIpData);

                if (BitConverter.IsLittleEndian)
                    destinationIpData = BinaryPrimitives.ReverseEndianness(destinationIpData);

                proxyInfo.DestinationIPAddress = new IPAddress(destinationIpData);

                reader.TryReadBigEndian(out UInt16 sourcePort);
                proxyInfo.SourcePort = sourcePort;

                reader.TryReadBigEndian(out UInt16 destinationPort);
                proxyInfo.DestinationPort = destinationPort;
            }
            else if (proxyInfo.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var addressBuffer = _bufferPool.Rent(IPV6_ADDRESS_ALL_LEN);

                try
                {
                    var addressBufferSpan = addressBuffer.AsSpan().Slice(0, IPV6_ADDRESS_ALL_LEN);

                    reader.TryCopyTo(addressBufferSpan);

                    proxyInfo.SourceIPAddress = new IPAddress(addressBufferSpan.Slice(0, IPV6_ADDRESS_LEN));
                    proxyInfo.DestinationIPAddress = new IPAddress(addressBufferSpan.Slice(IPV6_ADDRESS_LEN));

                    reader.Advance(IPV6_ADDRESS_ALL_LEN);
                }
                finally
                {
                    _bufferPool.Return(addressBuffer);
                }

                reader.TryReadBigEndian(out UInt16 sourcePort);
                proxyInfo.SourcePort = sourcePort;

                reader.TryReadBigEndian(out UInt16 destinationPort);
                proxyInfo.DestinationPort = destinationPort;
            }

            return true;
        }
    }
}