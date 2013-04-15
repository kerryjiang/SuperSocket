using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol.FramePartReader
{
    class MaskKeyReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            int required = lastLength + 4;

            if (frame.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            frame.MaskKey = frame.InnerData.ToArrayData(lastLength, 4);

            if (frame.ActualPayloadLength == 0)
            {
                nextPartReader = null;
                return (int)((long)frame.Length - required);
            }

            nextPartReader = new PayloadDataReader();

            if (frame.Length > required)
                return nextPartReader.Process(required, frame, out nextPartReader);

            return 0;
        }
    }
}
