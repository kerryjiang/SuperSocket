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
            ExtendedLengthReader = new ExtendedLengthReader();
            MaskKeyReader = new MaskKeyReader();
            PayloadDataReader = new PayloadDataReader();
        }

        public abstract bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData);

        public static IPackagePartReader<WebSocketPackage> NewReader
        {
            get { return FixPartReader; }
        }

        protected static IPackagePartReader<WebSocketPackage> FixPartReader { get; private set; }

        protected static IPackagePartReader<WebSocketPackage> ExtendedLengthReader { get; private set; }

        protected static IPackagePartReader<WebSocketPackage> MaskKeyReader { get; private set; }

        protected static IPackagePartReader<WebSocketPackage> PayloadDataReader { get; private set; }

        protected bool TryInitIfEmptyMessage(WebSocketPackage package)
        {
            if (package.PayloadLength != 0)
                return false;

            if (package.OpCode == OpCode.Text)
                package.Message = string.Empty;

            return true;
        }
    }
}
