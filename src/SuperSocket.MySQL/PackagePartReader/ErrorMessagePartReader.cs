using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.MySQL.PackagePartReader
{
    sealed class ErrorMessagePartReader : PackagePartReader
    {
        public override bool Process(QueryResult package, ref SequenceReader<byte> reader, out IPackagePartReader nextPartReader, out bool needMoreData)
        {
            nextPartReader = null;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> data, 0x00, false))
            {
                needMoreData = true;                
                return false;
            }

            reader.Advance(1);
            package.ErrorMessage = data.GetString(Encoding.UTF8);
            needMoreData = false;
            return true;
        }
    }
}
