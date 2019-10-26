using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.FramePartReader
{
    abstract class DataFramePartReader : IDataFramePartReader
    {
        static DataFramePartReader()
        {
            FixPartReader = new FixPartReader();
            ExtendedLenghtReader = new ExtendedLengthReader();
            MaskKeyReader = new MaskKeyReader();
            PayloadDataReader = new PayloadDataReader();
        }

        public abstract bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData);

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
