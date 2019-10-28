using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.PackagePartReader
{
    interface IPackagePartReader
    {
        bool Process(QueryResult package, ref SequenceReader<byte> reader, out IPackagePartReader nextPartReader, out bool needMoreData);
    }
}
