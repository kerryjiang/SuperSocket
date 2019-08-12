using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class PayloadDataReader : DataFramePartReader
    {
        public override int Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader)
        {
            long required = package.PayloadLength;

            if (reader.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            package.Data = reader.Sequence;
            nextPartReader = null;
            return 0;
        }
    }
}
