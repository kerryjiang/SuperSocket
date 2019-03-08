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
                
                _foundHeader = true;
                var header = reader.Sequence.Slice(0, _headerSize);
                var bodyLength = GetBodyLengthFromHeader(header);
                
                if (bodyLength < 0)
                {
                    throw new Exception("Failed to get body length from the package header.");
                }
                else if (bodyLength == 0)
                {
                    reader.Advance(_headerSize);
                    return DecodePackage(header);
                }
                else
                {
                    _totalSize = _headerSize + bodyLength;
                }
            }

            var totalSize = _totalSize;

            if (reader.Length > totalSize)
            {
                Reset();
                reader.Advance(totalSize);
                return DecodePackage(reader.Sequence.Slice(totalSize));
            }
            else if (reader.Length == totalSize)
            {
                Reset();
                reader.Advance(totalSize);                        
                return DecodePackage(reader.Sequence);
            }

            return null;           
        }
        
        protected abstract int GetBodyLengthFromHeader(ReadOnlySequence<byte> buffer);


        private void Reset()
        {
            _foundHeader = false;
            _headerSize = 0;
            _totalSize = 0;
        }
    }
}