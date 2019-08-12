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

        private WebSocketPackage _currentPackage;

        public WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            var package = _currentPackage;

            if (package == null)
            {
                _currentPackage = package = new WebSocketPackage();
                _currentPartReader = DataFramePartReader.NewReader;
            }

            if (!_currentPartReader.Process(package, ref reader, out IDataFramePartReader nextPartReader))
            {
                _currentPartReader = nextPartReader;
                return null;
            }

            Reset();
            return package;
        }

        public void Reset()
        {
            _currentPackage = null;
            _currentPartReader = null;
        }
    }
}
