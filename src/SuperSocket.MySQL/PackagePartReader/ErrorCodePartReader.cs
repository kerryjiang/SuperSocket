using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.PackagePartReader
{
    sealed class ErrorCodePartReader : PackagePartReader
    {
        public override bool Process(QueryResult package, ref SequenceReader<byte> reader, out IPackagePartReader nextPartReader, out bool needMoreData)
        {
            if (reader.Length < 2)
            {
                nextPartReader = null;
                needMoreData = true;
                return false;
            }

            reader.TryReadLittleEndian(out short errorCode);
            package.ErrorCode = errorCode;
            nextPartReader = ErrorMessagePartRealer;
            needMoreData = false;
            return false;
        }
    }
}
