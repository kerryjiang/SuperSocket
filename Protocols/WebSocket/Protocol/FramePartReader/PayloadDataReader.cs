using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol.FramePartReader
{
    class PayloadDataReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            long required = lastLength + frame.ActualPayloadLength;

            if (frame.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            nextPartReader = null;

            return (int)((long)frame.Length - required);
        }
    }
}
