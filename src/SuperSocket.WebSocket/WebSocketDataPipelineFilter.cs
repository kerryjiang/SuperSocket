using System;
using System.Buffers;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.FramePartReader;

namespace SuperSocket.WebSocket
{
    public class WebSocketDataPipelineFilter : PackagePartsPipelineFilter<WebSocketPackage>
    {
        private HttpHeader _httpHeader;

        /// <summary>
        /// -1: default value
        /// 0: ready to preserve bytes
        /// N: the bytes we preserved
        /// </summary>
        private long _consumed = -1;
        
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

        public override WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            WebSocketPackage package = default;
            var consumed = _consumed;

            if (consumed > 0)
            {
                var newReader = new SequenceReader<byte>(reader.Sequence);
                newReader.Advance(consumed);
                package = base.Filter(ref newReader);
                consumed = newReader.Consumed;
            }
            else
            {
                package = base.Filter(ref reader);
                // not final fragment or is the last fragment of multiple fragments message
                if (_consumed == 0)
                {
                    consumed = reader.Consumed;
                    reader.Rewind(consumed);
                }
            }
            
            if (consumed > 0)
            {
                if (_consumed < 0) // cleared
                    reader.Advance(consumed);
                else
                    _consumed = consumed;            
            }

            return package;
        }

        protected override IPackagePartReader<WebSocketPackage> GetFirstPartReader()
        {
            return PackagePartReader.NewReader;
        }

        protected override void OnPartReaderSwitched(IPackagePartReader<WebSocketPackage> currentPartReader, IPackagePartReader<WebSocketPackage> nextPartReader)
        {
            if (currentPartReader is FixPartReader)
            {
                // not final fragment or is the last fragment of multiple fragments message
                // _consumed = 0 means we are ready to preserve the bytes
                if (!CurrentPackage.FIN || CurrentPackage.Head != null)
                    _consumed = 0;
            }
        }

        public override void Reset()
        {
            _consumed = -1;            
            base.Reset();
        }
    }
}
