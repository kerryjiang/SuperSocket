using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter that processes packages with specific begin and end markers.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public abstract class BeginEndMarkPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        /// <summary>
        /// The marker indicating the beginning of a package.
        /// </summary>
        private readonly ReadOnlyMemory<byte> _beginMark;

        /// <summary>
        /// The marker indicating the end of a package.
        /// </summary>
        private readonly ReadOnlyMemory<byte> _endMark;

        /// <summary>
        /// Indicates whether the beginning marker has been found.
        /// </summary>
        private bool _foundBeginMark;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginEndMarkPipelineFilter{TPackageInfo}"/> class.
        /// </summary>
        /// <param name="beginMark">The marker indicating the beginning of a package.</param>
        /// <param name="endMark">The marker indicating the end of a package.</param>
        protected BeginEndMarkPipelineFilter(ReadOnlyMemory<byte> beginMark, ReadOnlyMemory<byte> endMark)
        {
            _beginMark = beginMark;
            _endMark = endMark;
        }

        /// <summary>
        /// Filters the data and extracts a package if both the begin and end markers are found.
        /// </summary>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <returns>The decoded package, or <c>null</c> if more data is needed.</returns>
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!_foundBeginMark)
            {
                var beginMark = _beginMark.Span;

                if (!reader.IsNext(beginMark, advancePast: true))
                {
                    throw new ProtocolException("Invalid beginning part of the package.");
                }

                _foundBeginMark = true;
            }

            var endMark = _endMark.Span;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, endMark, advancePastDelimiter: false))
            {
                return null;
            }

            reader.Advance(endMark.Length);
            return DecodePackage(ref pack);
        }

        /// <summary>
        /// Resets the state of the pipeline filter.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _foundBeginMark = false;
        }
    }
}