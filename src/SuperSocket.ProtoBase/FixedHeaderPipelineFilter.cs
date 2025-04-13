using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter that processes packages with a fixed header size.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public abstract class FixedHeaderPipelineFilter<TPackageInfo> : FixedSizePipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        /// <summary>
        /// Indicates whether the header has been found.
        /// </summary>
        private bool _foundHeader;

        /// <summary>
        /// The size of the header.
        /// </summary>
        private readonly int _headerSize;

        /// <summary>
        /// The total size of the package, including the header and body.
        /// </summary>
        private int _totalSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedHeaderPipelineFilter{TPackageInfo}"/> class with the specified header size.
        /// </summary>
        /// <param name="headerSize">The size of the header.</param>
        protected FixedHeaderPipelineFilter(int headerSize)
            : base(headerSize)
        {
            _headerSize = headerSize;
        }

        /// <summary>
        /// Filters the data and extracts a package if the header and body are complete.
        /// </summary>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <returns>The decoded package, or <c>null</c> if more data is needed.</returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!_foundHeader)
            {
                if (reader.Length < _headerSize)
                    return null;

                var header = reader.Sequence.Slice(0, _headerSize);
                var bodyLength = GetBodyLengthFromHeader(ref header);

                if (bodyLength < 0)
                    throw new ProtocolException("Failed to get body length from the package header.");

                if (bodyLength == 0)
                {
                    try
                    {
                        return DecodePackage(ref header);
                    }
                    finally
                    {
                        reader.Advance(_headerSize);
                    }
                }

                _foundHeader = true;
                _totalSize = _headerSize + bodyLength;
            }

            var totalSize = _totalSize;

            if (reader.Length < totalSize)
                return null;

            var pack = reader.Sequence.Slice(0, totalSize);

            try
            {
                return DecodePackage(ref pack);
            }
            finally
            {
                reader.Advance(totalSize);
            }
        }

        /// <summary>
        /// Gets the body length from the header.
        /// </summary>
        /// <param name="buffer">The buffer containing the header.</param>
        /// <returns>The length of the body.</returns>
        protected abstract int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer);

        /// <summary>
        /// Resets the state of the pipeline filter.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _foundHeader = false;
            _totalSize = 0;
        }
    }
}