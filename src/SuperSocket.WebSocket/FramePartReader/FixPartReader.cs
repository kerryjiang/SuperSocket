using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class FixPartReader : DataFramePartReader
    {
        public override bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
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
