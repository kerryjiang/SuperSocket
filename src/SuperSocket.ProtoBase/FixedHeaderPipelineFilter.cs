using System;
using System.Buffers;
using System.Collections;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    public abstract class FixedHeaderPipelineFilter<TPackageInfo> : FixedSizePipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        private bool _foundHeader = false;
        private int _headerSize;

        public FixedHeaderPipelineFilter(int headerSize)
            : base(headerSize)
        {
            _headerSize = headerSize;
        }
        
        protected abstract int GetBodyLengthFromHeader(ReadOnlySequence<byte> buffer);


        protected override bool CanDecodePackage(ReadOnlySequence<byte> buffer)
        {
            if (_foundHeader)
                return true;

            var bodyLength = GetBodyLengthFromHeader(buffer);

            if (bodyLength < 0)
            {
                return false;
            }

            // no body part
            if (bodyLength == 0)
            {
                _foundHeader = true;
                return true;
            }

            ResetSize(_headerSize + bodyLength);
            _foundHeader = true;
            return false;
        }
    }
}