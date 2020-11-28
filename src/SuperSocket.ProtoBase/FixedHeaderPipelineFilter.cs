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
        
        protected abstract int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer);

        public override void Reset()
        {
            base.Reset();
            
            _foundHeader = false;
            _totalSize = 0;
        }
    }
}