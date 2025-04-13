using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    /// <summary>
    /// Represents a reader for processing the extended length field of a WebSocket package.
    /// </summary>
    class ExtendedLengthReader : PackagePartReader
    {
        /// <summary>
        /// Processes the extended length field of a WebSocket package.
        /// </summary>
        /// <param name="package">The WebSocket package being processed.</param>
        /// <param name="filterContext">The context of the pipeline filter.</param>
        /// <param name="reader">The sequence reader for the extended length data.</param>
        /// <param name="nextPartReader">The next part reader to process subsequent parts.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete processing.</param>
        /// <returns>True if the extended length field is fully processed; otherwise, false.</returns>
        public override bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData)
        {
            int required;

            if (package.PayloadLength == 126)
                required = 2;
            else
                required = 8;

            if (reader.Remaining < required)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            if (required == 2)
            {
                reader.TryReadBigEndian(out ushort len);
                package.PayloadLength = len;
            }
            else // required == 8 (long)
            {
                reader.TryReadBigEndian(out long len);
                package.PayloadLength = len;
            }

            if (package.HasMask)
                nextPartReader = MaskKeyReader;
            else
                nextPartReader = PayloadDataReader;

            return false;
        }
    }
}
