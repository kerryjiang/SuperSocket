using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.PackagePartReader
{
    abstract class PackagePartReader : IPackagePartReader
    {
        public static IPackagePartReader PackageHeadReader { get; private set; }

        public static IPackagePartReader ErrorCodePartReader { get; private set; }

        public static IPackagePartReader ErrorMessagePartRealer { get; private set; }

        static PackagePartReader()
        {
            PackageHeadReader = new PackageHeadReader();
            ErrorCodePartReader = new ErrorCodePartReader();
            ErrorMessagePartRealer = new ErrorMessagePartReader();
        }

        internal static IPackagePartReader NewReader
        {
            get { return PackageHeadReader;  }
        }

        public abstract bool Process(QueryResult package, ref SequenceReader<byte> reader, out IPackagePartReader nextPartReader, out bool needMoreData);    
        
    }
}
