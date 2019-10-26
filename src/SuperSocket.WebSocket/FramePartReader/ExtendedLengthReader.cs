using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class ExtendedLengthReader : DataFramePartReader
    {
        public override bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
        {
            int required;

            if (package.PayloadLength == 126)
                required = 2;
            else
                required = 8;

            if (reader.Length < required)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            if (required == 2)
            {
                reader.TryReadBigEndian(out short len);
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
