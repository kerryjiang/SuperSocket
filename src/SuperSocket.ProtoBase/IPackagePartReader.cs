using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Defines the functionality of a package part reader.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IPackagePartReader<TPackageInfo>
    {
        /// <summary>
        /// Processes a part of the package.
        /// </summary>
        /// <param name="package">The package being processed.</param>
        /// <param name="filterContext">The context of the filter.</param>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <param name="nextPartReader">The next part reader to use.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete the package.</param>
        /// <returns><c>true</c> if the package is complete; otherwise, <c>false</c>.</returns>
        bool Process(TPackageInfo package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData);
    }
}