using System;
using System.Buffers;
using System.Linq;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    /// <summary>
    /// Represents a reader for processing the mask key field of a WebSocket package.
    /// </summary>
    class MaskKeyReader : PackagePartReader
    {
        /// <summary>
        /// Processes the mask key field of a WebSocket package.
        /// </summary>
        /// <param name="package">The WebSocket package being processed.</param>
        /// <param name="filterContext">The context of the pipeline filter.</param>
        /// <param name="reader">The sequence reader for the mask key data.</param>
        /// <param name="nextPartReader">The next part reader to process subsequent parts.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete processing.</param>
        /// <returns>True if the mask key field is fully processed; otherwise, false.</returns>
        public override bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData)
        {
            int required = 4;

            if (reader.Remaining < required)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            package.MaskKey = reader.Sequence.Slice(reader.Consumed, 4).ToArray();
            reader.Advance(4);

            if (TryInitIfEmptyMessage(package))
            {
                nextPartReader = null;
                return true;
            }

            nextPartReader = PayloadDataReader;
            return false;
        }
    }
}
