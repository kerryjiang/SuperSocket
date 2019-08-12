using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class MaskKeyReader : DataFramePartReader
    {
        public override int Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader)
        {
            int required = 4;

            if (reader.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            package.MaskKey = reader.Sequence.Slice(0, 4).ToArray();
            reader.Advance(4);

            nextPartReader = PayloadDataReader;
            return 0;
        }
    }
}
