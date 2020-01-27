using System;
using System.Buffers;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.FramePartReader;

namespace SuperSocket.WebSocket
{
    public class WebSocketDataPipelineFilter : PackagePartsPipelineFilter<WebSocketPackage>
    {
        private HttpHeader _httpHeader;
        
        public WebSocketDataPipelineFilter(HttpHeader httpHeader)
        {
            _httpHeader = httpHeader;
        }

        protected override WebSocketPackage CreatePackage()
        {
            return new WebSocketPackage
            {
                HttpHeader = _httpHeader
            };
        }

        protected override IPackagePartReader<WebSocketPackage> GetFirstPartReader()
        {
            return PackagePartReader.NewReader;
        }
    }
}
