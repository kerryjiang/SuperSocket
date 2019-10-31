using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class FixedHeaderPipelineFilter<TPackageInfo> : FixedSizePipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        private bool _foundHeader;
        private readonly int _headerSize;
        private int _totalSize;

        protected FixedHeaderPipelineFilter(int headerSize)
            : base(headerSize)
        {
            _headerSize = headerSize;
        }

        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!_foundHeader)
            {
                if (reader.Length < _headerSize)
                    return null;                
                
                var header = reader.Sequence.Slice(0, _headerSize);
                var bodyLength = GetBodyLengthFromHeader(header);
                
                if (bodyLength < 0)
                    throw new ProtocolException("Failed to get body length from the package header.");
                
                if (bodyLength == 0)
                    return DecodePackage(header);
                
                _foundHeader = true;
                _totalSize = _headerSize + bodyLength;

                return Filter(ref reader);
            }

            var totalSize = _totalSize;

            var sequence = reader.Sequence;

            if (reader.Length > totalSize)
            {
                sequence = sequence.Slice(0, totalSize);
            }

            var package = DecodePackage(sequence);

            // mark the data consumed
            reader.Advance(totalSize);

            return package;        
        }
        
        protected abstract int GetBodyLengthFromHeader(ReadOnlySequence<byte> buffer);

        public override void Reset()
        {
            _foundHeader = false;
            _totalSize = 0;
        }
    }
}