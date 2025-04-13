using System;
using System.Buffers;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    /// <summary>
    /// An abstract base class for reading parts of a proxy protocol package.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    abstract class ProxyProtocolPackagePartReader<TPackageInfo> : IPackagePartReader<TPackageInfo>
    {
        static ProxyProtocolPackagePartReader()
        {
            ProxyProtocolSwitch = new ProxyProtocolSwitchPartReader<TPackageInfo>();
            ProxyProtocolV1Reader = new ProxyProtocolV1PartReader<TPackageInfo>();
            ProxyProtocolV2Reader = new ProxyProtocolV2PartReader<TPackageInfo>();
        }

        /// <summary>
        /// Processes a part of the package and determines the next part reader.
        /// </summary>
        /// <param name="package">The package being processed.</param>
        /// <param name="filterContext">The context for the filter.</param>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <param name="nextPartReader">The next part reader to use.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete processing.</param>
        /// <returns><c>true</c> if processing was successful; otherwise, <c>false</c>.</returns>
        public abstract bool Process(TPackageInfo package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData);

        /// <summary>
        /// Gets the part reader for switching between proxy protocol versions.
        /// </summary>
        internal static IPackagePartReader<TPackageInfo> ProxyProtocolSwitch { get; }

        /// <summary>
        /// Gets the part reader for proxy protocol version 1.
        /// </summary>
        internal static IPackagePartReader<TPackageInfo> ProxyProtocolV1Reader { get; }

        /// <summary>
        /// Gets the part reader for proxy protocol version 2.
        /// </summary>
        internal static IPackagePartReader<TPackageInfo> ProxyProtocolV2Reader { get; }
    }
}