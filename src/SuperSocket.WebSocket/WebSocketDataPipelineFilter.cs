using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketDataPipelineFilter : IPipelineFilter<WebSocketPackage>
    {
        public IPackageDecoder<WebSocketPackage> Decoder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IPipelineFilter<WebSocketPackage> NextFilter => throw new NotImplementedException();

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
