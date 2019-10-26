using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    interface IDataFramePartReader
    {
        bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData);
    }
}
