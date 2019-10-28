using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.FramePartReader
{
    sealed class ErrorCodePartReader : DataFramePartReader
    {
        public override bool Process(QueryResult package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
        {
            if (reader.Length < 2)
            {
                nextPartReader = null;
                needMoreData = true;
                return null;
            }

            reader.TryRead(out short errorCode);
            package.ErrorCode = errorCode;
            nextPartReader = ErrorMessagePartRealer;
        }
    }
}
