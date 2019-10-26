using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class MaskKeyReader : DataFramePartReader
    {
        public override bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
        {
            int required = 4;

            if (reader.Length < required)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            package.MaskKey = reader.Sequence.Slice(0, 4).ToArray();
            reader.Advance(4);

            nextPartReader = PayloadDataReader;
            return false;
        }
    }
}
