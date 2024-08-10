using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.VisualBasic;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
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
            
            LoadProxyInfo(filterContext as ProxyInfo, proxyLine, 12, 13);
            
            return true;
        }

        private void LoadProxyInfo(ProxyInfo proxyInfo, string line, int startPos, int lookForOffet)
        {
            var span = line.AsSpan();
            var segmentIndex = 0;

            while (lookForOffet < line.Length)
            {
                var spacePos = line.IndexOf(' ', lookForOffet);

                if (spacePos < 0)
                    break;

                startPos = spacePos + 1;
                lookForOffet = startPos + 1;

                var segment = span.Slice(startPos, spacePos - startPos);

                PROXY_SEGMENT_PARSERS[segmentIndex++].Process(segment, proxyInfo);
            }
        }

        interface IProxySgementProcessor
        {
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
                proxyInfo.SourcePort = int.Parse(segment);
            }
        }
    }
}