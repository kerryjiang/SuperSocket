using System;
using System.Buffers;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.FramePartReader;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents a pipeline filter for processing WebSocket data.
    /// </summary>
    public class WebSocketDataPipelineFilter : PackagePartsPipelineFilter<WebSocketPackage>
    {
        private readonly HttpHeader _httpHeader;

        private readonly bool _requireMask = true;

        /// <summary>
        /// -1: default value
        /// 0: ready to preserve bytes
        /// N: the bytes we preserved
        /// </summary>
        private long _consumed = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketDataPipelineFilter"/> class.
        /// </summary>
        /// <param name="httpHeader">The HTTP header associated with the WebSocket package.</param>
        /// <param name="requireMask">Indicates whether a mask is required for WebSocket packages.</param>
        public WebSocketDataPipelineFilter(HttpHeader httpHeader, bool requireMask = true)
        {
            _httpHeader = httpHeader;
            _requireMask = requireMask;
        }

        /// <summary>
        /// Creates a new WebSocket package.
        /// </summary>
        /// <returns>A new instance of <see cref="WebSocketPackage"/>.</returns>
        protected override WebSocketPackage CreatePackage()
        {
            return new WebSocketPackage
            {
                HttpHeader = _httpHeader
            };
        }

        /// <summary>
        /// Filters the incoming data and returns a WebSocket package.
        /// </summary>
        /// <param name="reader">The sequence reader for the incoming data.</param>
        /// <returns>The filtered WebSocket package.</returns>
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

        /// <summary>
        /// Gets the first part reader for processing WebSocket packages.
        /// </summary>
        /// <returns>The first part reader.</returns>
        protected override IPackagePartReader<WebSocketPackage> GetFirstPartReader()
        {
            return PackagePartReader.NewReader;
        }

        /// <summary>
        /// Handles the event when the part reader is switched.
        /// </summary>
        /// <param name="currentPartReader">The current part reader.</param>
        /// <param name="nextPartReader">The next part reader.</param>
        protected override void OnPartReaderSwitched(IPackagePartReader<WebSocketPackage> currentPartReader, IPackagePartReader<WebSocketPackage> nextPartReader)
        {
            if (currentPartReader is FixPartReader)
            {
                if (_requireMask && !CurrentPackage.HasMask)
                {
                    throw new ProtocolException("Mask is required for this websocket package.");
                }

                // not final fragment or is the last fragment of multiple fragments message
                // _consumed = 0 means we are ready to preserve the bytes
                if (!CurrentPackage.FIN || CurrentPackage.Head != null)
                    _consumed = 0;
            }
        }

        /// <summary>
        /// Resets the pipeline filter to its initial state.
        /// </summary>
        public override void Reset()
        {
            _consumed = -1;            
            base.Reset();
        }
    }
}
