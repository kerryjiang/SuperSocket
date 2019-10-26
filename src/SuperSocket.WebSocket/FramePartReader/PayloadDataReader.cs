using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class PayloadDataReader : DataFramePartReader
    {
        public override bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
        {
            nextPartReader = null;

            long required = package.PayloadLength;

            if (reader.Length < required)
            {                
                needMoreData = true;
                return false;
            }

            needMoreData = false;
            package.Data = reader.Sequence.Slice(0, required);
            reader.Advance(required);
            
            return true;
        }
    }
}
