using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter that transparently processes packages without modifying the data.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class TransparentPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        /// <summary>
        /// Filters the data and extracts a package from the sequence reader.
        /// </summary>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <returns>The extracted package.</returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var sequence = reader.Sequence;
            var total = reader.Remaining;
            var package = DecodePackage(ref sequence);
            reader.Advance(total);
            return package;
        }
    }
}
