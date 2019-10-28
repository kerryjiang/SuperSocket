using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.FramePartReader
{
    sealed class ErrorMessagePartReader : DataFramePartReader
    {
        public override bool Process(QueryResult package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
        {
            nextPartReader = null;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> data, 0x00, false))
            {
                needMoreData = true;                
                return false;
            }

            reader.Advance(1);
            package.ErrorMessage = Encoding.UTF8.GetString(data);
            return true;
        }
    }
}
