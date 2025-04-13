using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    /// <summary>
    /// Represents a reader for processing the fixed part of a WebSocket package.
    /// </summary>
    class FixPartReader : PackagePartReader
    {
        /// <summary>
        /// Processes the fixed part of a WebSocket package.
        /// </summary>
        /// <param name="package">The WebSocket package being processed.</param>
        /// <param name="filterContext">The context of the pipeline filter.</param>
        /// <param name="reader">The sequence reader for the fixed part data.</param>
        /// <param name="nextPartReader">The next part reader to process subsequent parts.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete processing.</param>
        /// <returns>True if the fixed part is fully processed; otherwise, false.</returns>
        public override bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData)
        {
            if (reader.Length < 2)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            reader.TryRead(out byte firstByte);

            var opCode = (OpCode)(firstByte & 0x0f);

            if (opCode != OpCode.Continuation)
            {
                package.OpCode = opCode;
            }

            package.OpCodeByte = firstByte;

            reader.TryRead(out byte secondByte);
            package.PayloadLength = secondByte & 0x7f;
            package.HasMask = (secondByte & 0x80) == 0x80;

            if (package.PayloadLength >= 126)
            {
                nextPartReader = ExtendedLengthReader;
            }
            else
            {
                if (package.HasMask)
                    nextPartReader = MaskKeyReader;
                else
                {
                    if (TryInitIfEmptyMessage(package))
                    {
                        nextPartReader = null;
                        return true;
                    }

                    nextPartReader = PayloadDataReader;
                }
            }

            return false;
        }
    }
}
