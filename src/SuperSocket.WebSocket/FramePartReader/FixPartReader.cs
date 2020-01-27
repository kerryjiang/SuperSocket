using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    class FixPartReader : PackagePartReader
    {
        public override bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData)
        {
            if (reader.Length < 2)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            reader.TryRead(out byte firstByte);
            package.OpCode = (OpCode)(firstByte & 0x0f);
            package.OpCodeByte = firstByte;

            reader.TryRead(out byte secondByte);
            package.PayloadLength = secondByte & 0x7f;
            package.HasMask = (secondByte & 0x80) == 0x80;

            if (package.PayloadLength >= 126)
            {
                nextPartReader = ExtendedLenghtReader;
            }
            else
            {
                if (package.HasMask)
                    nextPartReader = MaskKeyReader;
                else
                {
                    // no body
                    if (package.PayloadLength == 0)
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
