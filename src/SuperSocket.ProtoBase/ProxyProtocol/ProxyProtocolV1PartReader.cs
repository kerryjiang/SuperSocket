using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    /// <summary>
    /// Processes proxy protocol version 1 headers.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    class ProxyProtocolV1PartReader<TPackageInfo> : ProxyProtocolPackagePartReader<TPackageInfo>
    {
        private static readonly byte[] PROXY_DELIMITER = Encoding.ASCII.GetBytes("\r\n");

        private static readonly IProxySgementProcessor[] PROXY_SEGMENT_PARSERS = new IProxySgementProcessor[]
            {
                new SourceIPAddressProcessor(),
                new DestinationIPAddressProcessor(),
                new SourcePortProcessor(),
                new DestinationPortProcessor()
            };

        /// <summary>
        /// Processes the proxy protocol version 1 header and extracts connection information.
        /// </summary>
        /// <param name="package">The package being processed.</param>
        /// <param name="filterContext">The context for the filter.</param>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <param name="nextPartReader">The next part reader to use.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete processing.</param>
        /// <returns><c>true</c> if processing was successful; otherwise, <c>false</c>.</returns>
        public override bool Process(TPackageInfo package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData)
        {
            if (!reader.TryReadTo(out ReadOnlySequence<byte> proxyLineSequence, PROXY_DELIMITER, true))
            {
                needMoreData = true;
                nextPartReader = null;
                return false;
            }

            needMoreData = false;
            nextPartReader = null;

            var proxyLineReader = new SequenceReader<byte>(proxyLineSequence);

            var proxyLine = proxyLineReader.ReadString();

            var proxyInfo = filterContext as ProxyInfo;

            // "PROXY TCP4 X", start look for next segment from X(@11)
            LoadProxyInfo(proxyInfo, proxyLine, 11, 12);

            proxyInfo.Version = 1;
            proxyInfo.Command = ProxyCommand.PROXY;
            proxyInfo.ProtocolType = ProtocolType.Tcp;

            return true;
        }

        /// <summary>
        /// Loads proxy information from the proxy line.
        /// </summary>
        /// <param name="proxyInfo">The proxy information object to update.</param>
        /// <param name="line">The proxy line containing connection details.</param>
        /// <param name="startPos">The starting position for parsing.</param>
        /// <param name="lookForOffet">The offset for looking for the next segment.</param>
        private void LoadProxyInfo(ProxyInfo proxyInfo, string line, int startPos, int lookForOffet)
        {
            var span = line.AsSpan();
            var segmentIndex = 0;

            while (lookForOffet < line.Length)
            {
                var spacePos = line.IndexOf(' ', lookForOffet);

                ReadOnlySpan<char> segment;

                if (spacePos >= 0)
                {
                    segment = span.Slice(startPos, spacePos - startPos);
                    startPos = spacePos + 1;
                    lookForOffet = startPos + 1;
                }
                else
                {
                    segment = span.Slice(startPos);
                    lookForOffet = line.Length;
                }

                PROXY_SEGMENT_PARSERS[segmentIndex++].Process(segment, proxyInfo);
            }
        }

        /// <summary>
        /// Defines a processor for handling segments of the proxy line.
        /// </summary>
        interface IProxySgementProcessor
        {
            /// <summary>
            /// Processes a segment of the proxy line.
            /// </summary>
            /// <param name="segment">The segment to process.</param>
            /// <param name="proxyInfo">The proxy information object to update.</param>
            void Process(ReadOnlySpan<char> segment, ProxyInfo proxyInfo);
        }

        class SourceIPAddressProcessor : IProxySgementProcessor
        {
            public void Process(ReadOnlySpan<char> segment, ProxyInfo proxyInfo)
            {
                proxyInfo.SourceIPAddress = IPAddress.Parse(segment);
            }
        }

        class DestinationIPAddressProcessor : IProxySgementProcessor
        {
            public void Process(ReadOnlySpan<char> segment, ProxyInfo proxyInfo)
            {
                proxyInfo.DestinationIPAddress = IPAddress.Parse(segment);
            }
        }

        class SourcePortProcessor : IProxySgementProcessor
        {
            public void Process(ReadOnlySpan<char> segment, ProxyInfo proxyInfo)
            {
                proxyInfo.SourcePort = int.Parse(segment);
            }
        }

        class DestinationPortProcessor : IProxySgementProcessor
        {
            public void Process(ReadOnlySpan<char> segment, ProxyInfo proxyInfo)
            {
                proxyInfo.DestinationPort = int.Parse(segment);
            }
        }
    }
}