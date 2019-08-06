using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    class ExtendedLengthReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            int required = 2;

            if (frame.PayloadLength == 126)
                required += 2;
            else
                required += 8;

            if (frame.Length < required)
            {
                nextPartReader = this;
                return -1;
            }

            if (frame.HasMask)
                nextPartReader = MaskKeyReader;
            else
            {
                if (frame.ActualPayloadLength == 0)
                {
                    nextPartReader = null;
                    return (int)((long)frame.Length - required);
                }

                nextPartReader = PayloadDataReader;
            }

            if (frame.Length > required)
                return nextPartReader.Process(required, frame, out nextPartReader);

            return 0;
        }
    }
}
