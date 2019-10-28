using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.FramePartReader
{
    abstract class DataFramePartReader : IDataFramePartReader
    {
        public static IDataFramePartReader PackageHeadReader { get; private set; }

        public static IDataFramePartReader ErrorCodePartReader { get; private set; }

        public static IDataFramePartReader ErrorMessagePartRealer { get; private set; }

        static DataFramePartReader()
        {
            PackageHeadReader = new PackageHeadReader();
            ErrorCodePartReader = new ErrorCodePartReader();
            ErrorMessagePartRealer = new ErrorMessageReader();
        }

        internal static IDataFramePartReader NewReader
        {
            get { return PackageHeadReader;  }
        }

        abstract bool Process(QueryResult package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData);    
        
    }
}
