using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol.FramePartReader
{
    abstract class DataFramePartReader : IDataFramePartReader
    {
        static DataFramePartReader()
        {
            FixPartReader = new FixPartReader();
            ExtendedLenghtReader = new ExtendedLenghtReader();
            MaskKeyReader = new MaskKeyReader();
            PayloadDataReader = new PayloadDataReader();
        }

        public abstract int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader);

        public static IDataFramePartReader NewReader
        {
            get { return FixPartReader; }
        }

        protected static IDataFramePartReader FixPartReader { get; private set; }

        protected static IDataFramePartReader ExtendedLenghtReader { get; private set; }

        protected static IDataFramePartReader MaskKeyReader { get; private set; }

        protected static IDataFramePartReader PayloadDataReader { get; private set; }
    }
}
