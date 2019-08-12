using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class ExtendedLengthReader : DataFramePartReader
    {
        public override bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader)
        {
            int required;

            if (package.PayloadLength == 126)
                required = 2;
            else
                required = 8;

            if (reader.Length < required)
            {
                nextPartReader = this;
                return false;
            }

            if (required == 2)
            {
                reader.TryReadLittleEndian(out short len);
                package.PayloadLength = len;
            }
            else // required == 8 (long)
            {
                reader.TryReadLittleEndian(out long len);
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
