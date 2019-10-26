using System;
using System.Buffers;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.FramePartReader;

namespace SuperSocket.WebSocket
{
    public class WebSocketDataPipelineFilter : IPipelineFilter<WebSocketPackage>
    {
        public IPackageDecoder<WebSocketPackage> Decoder { get; set; } 

        public IPipelineFilter<WebSocketPackage> NextFilter => null;

        private IDataFramePartReader _currentPartReader;

        private HttpHeader _httpHeader;

        private WebSocketPackage _currentPackage;

        public WebSocketDataPipelineFilter(HttpHeader httpHeader)
        {
            _httpHeader = httpHeader;
        }

        public WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            var package = _currentPackage;

            if (package == null)
            {
                package = _currentPackage = new WebSocketPackage { HttpHeader = _httpHeader };
                _currentPartReader = DataFramePartReader.NewReader;
            }

            while (true)
            {
                if (_currentPartReader.Process(package, ref reader, out IDataFramePartReader nextPartReader, out bool needMoreData))
                {
                    Reset();
                    return package;
                }

                if (nextPartReader != null)
                    _currentPartReader = nextPartReader;

                if (needMoreData || reader.Remaining <= 0)
                    return null;
            }
        }

        public void Reset()
        {
            _currentPackage = null;
            _currentPartReader = null;
        }
    }
}
