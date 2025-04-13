using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    /// <summary>
    /// Represents the base class for reading parts of a WebSocket package.
    /// </summary>
    abstract class PackagePartReader : IPackagePartReader<WebSocketPackage>
    {
        /// <summary>
        /// Initializes static members of the <see cref="PackagePartReader"/> class.
        /// </summary>
        static PackagePartReader()
        {
            FixPartReader = new FixPartReader();
            ExtendedLengthReader = new ExtendedLengthReader();
            MaskKeyReader = new MaskKeyReader();
            PayloadDataReader = new PayloadDataReader();
        }

        /// <summary>
        /// Processes a part of the WebSocket package.
        /// </summary>
        /// <param name="package">The WebSocket package being processed.</param>
        /// <param name="filterContext">The context of the pipeline filter.</param>
        /// <param name="reader">The sequence reader for the package data.</param>
        /// <param name="nextPartReader">The next part reader to process subsequent parts.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete processing.</param>
        /// <returns>True if the part is fully processed; otherwise, false.</returns>
        public abstract bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData);

        /// <summary>
        /// Gets the initial reader for processing WebSocket packages.
        /// </summary>
        public static IPackagePartReader<WebSocketPackage> NewReader
        {
            get { return FixPartReader; }
        }

        /// <summary>
        /// Gets the reader for fixed parts of a WebSocket package.
        /// </summary>
        protected static IPackagePartReader<WebSocketPackage> FixPartReader { get; private set; }

        /// <summary>
        /// Gets the reader for extended length parts of a WebSocket package.
        /// </summary>
        protected static IPackagePartReader<WebSocketPackage> ExtendedLengthReader { get; private set; }

        /// <summary>
        /// Gets the reader for mask key parts of a WebSocket package.
        /// </summary>
        protected static IPackagePartReader<WebSocketPackage> MaskKeyReader { get; private set; }

        /// <summary>
        /// Gets the reader for payload data parts of a WebSocket package.
        /// </summary>
        protected static IPackagePartReader<WebSocketPackage> PayloadDataReader { get; private set; }

        /// <summary>
        /// Initializes the package if it represents an empty message.
        /// </summary>
        /// <param name="package">The WebSocket package to initialize.</param>
        /// <returns>True if the package is initialized as an empty message; otherwise, false.</returns>
        protected bool TryInitIfEmptyMessage(WebSocketPackage package)
        {
            if (package.PayloadLength != 0)
                return false;

            // This fragment is empty doesn't mean the whole message is empty
            if (package.Head != null)
                return false;

            if (package.OpCode == OpCode.Text)
                package.Message = string.Empty;

            return true;
        }
    }
}
