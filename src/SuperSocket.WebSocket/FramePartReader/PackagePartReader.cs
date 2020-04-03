using System;
using System.Buffers;
using SuperSocket.ProtoBase;


namespace SuperSocket.WebSocket.FramePartReader
{
    abstract class PackagePartReader : IPackagePartReader<WebSocketPackage>
    {
        static PackagePartReader()
        {
            FixPartReader = new FixPartReader();
            ExtendedLenghtReader = new ExtendedLengthReader();
            MaskKeyReader = new MaskKeyReader();
            PayloadDataReader = new PayloadDataReader();
        }

        public abstract bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData);

        public static IPackagePartReader<WebSocketPackage> NewReader
        {
            get { return FixPartReader; }
        }

        protected static IPackagePartReader<WebSocketPackage> FixPartReader { get; private set; }

        protected static IPackagePartReader<WebSocketPackage> ExtendedLenghtReader { get; private set; }

        protected static IPackagePartReader<WebSocketPackage> MaskKeyReader { get; private set; }

        protected static IPackagePartReader<WebSocketPackage> PayloadDataReader { get; private set; }

        protected bool CheckIfEmptyMessage(WebSocketPackage package)
        {
            return package.PayloadLength == 0;
        }
    }
}
