using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

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

            var seq = reader.Sequence.Slice(0, required);

            if (package.OpCode == OpCode.Binary)
                package.Data = seq;
            else
                package.Message = seq.GetString(Encoding.UTF8);

            reader.Advance(required);
            
            return true;
        }
    }
}
