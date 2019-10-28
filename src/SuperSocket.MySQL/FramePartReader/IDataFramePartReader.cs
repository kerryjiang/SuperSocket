using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.FramePartReader
{
    interface IDataFramePartReader
    {
        bool Process(QueryResult package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData);
    }
}
