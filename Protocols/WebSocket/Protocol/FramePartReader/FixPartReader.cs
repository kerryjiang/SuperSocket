using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol.FramePartReader
{
    class FixPartReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            if (frame.Length < 2)
            {
                nextPartReader = this;
                return -1;
            }

            if (frame.PayloadLenght < 126)
            {
                if (frame.HasMask)
                    nextPartReader = MaskKeyReader;
                else
                {
                    if (frame.ActualPayloadLength == 0)
                    {
                        nextPartReader = null;
                        return (int)((long)frame.Length - 2);
                    }

                    nextPartReader = PayloadDataReader;
                }
            }
            else
            {
                nextPartReader = ExtendedLenghtReader;
            }

            if (frame.Length > 2)
                return nextPartReader.Process(2, frame, out nextPartReader);

            return 0;
        }
    }
}
