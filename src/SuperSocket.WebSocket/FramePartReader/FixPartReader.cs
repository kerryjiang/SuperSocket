using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class FixPartReader : DataFramePartReader
    {
        public override int Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader)
        {
            if (reader.Length < 2)
            {
                nextPartReader = this;
                return -1;
            }

            reader.TryRead(out byte firstByte);
            package.OpCode = (OpCode)firstByte;
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
                        return 0;
                    }

                    nextPartReader = PayloadDataReader;
                }
            }

            return 0;
        }
    }
}
