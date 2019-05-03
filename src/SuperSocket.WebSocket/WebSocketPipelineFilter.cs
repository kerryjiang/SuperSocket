using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketPipelineFilter : IPipelineFilter<WebSocketPackage>
    {
        public IPackageDecoder<WebSocketPackage> Decoder { get; set; }

        public IPipelineFilter<WebSocketPackage> NextFilter { get; set; }

        public WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
