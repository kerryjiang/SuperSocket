using System;
using System.Buffers;
using System.Collections.Specialized;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketPipelineFilter : IPipelineFilter<WebSocketPackage>
    {
        private static readonly ReadOnlyMemory<byte> _headerTerminator = Encoding.UTF8.GetBytes("\r\n\r\n");
        
        public IPackageDecoder<WebSocketPackage> Decoder { get; set; }

        public IPipelineFilter<WebSocketPackage> NextFilter { get; set; }

        public WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            var terminatorSpan = _headerTerminator.Span;

            if (!reader.TryReadToAny(out ReadOnlySequence<byte> pack, terminatorSpan, advancePastDelimiter: false))
            {
                return null;
            }

            for (var i = 0; i < terminatorSpan.Length - 1; i++)
            {
                if (!reader.IsNext(terminatorSpan, advancePast: true))
                {
                    return null;
                }
            }

            //var headerItems = ParseHttpHeaderItems(pack);

            //var secWebSocketVersion = headerItems[WebSocketConstant.SecWebSocketVersion];
            //var secWebSocketKey = headerItems[WebSocketConstant.SecWebSocketKey];

            return null;
        }

        private NameValueCollection ParseHttpHeaderItems(ReadOnlySequence<byte> header)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
