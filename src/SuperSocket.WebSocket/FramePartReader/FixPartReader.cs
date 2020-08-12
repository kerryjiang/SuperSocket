using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    class FixPartReader : PackagePartReader
    {
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
