using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter that processes fixed-size packages.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class FixedSizePipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private int _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedSizePipelineFilter{TPackageInfo}"/> class with the specified package size.
        /// </summary>
        /// <param name="size">The size of the fixed-size package.</param>
        protected FixedSizePipelineFilter(int size)
        {
            _size = size;
        }

        /// <summary>
        /// Filters the buffer to extract a fixed-size package.
        /// </summary>
        /// <param name="reader">The sequence reader containing the buffer data.</param>
        /// <returns>The decoded package, or <c>null</c> if the buffer does not contain enough data.</returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (reader.Length < _size)
                return null;

            var pack = reader.Sequence.Slice(0, _size);

            try
            {
                return DecodePackage(ref pack);
            }
            finally
            {
                reader.Advance(_size);
            }
        }
    }
}