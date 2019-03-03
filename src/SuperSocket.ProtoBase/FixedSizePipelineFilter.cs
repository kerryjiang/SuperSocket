using System;
using System.Buffers;
using System.Collections;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    public abstract class FixedSizePipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private int _size;

        public FixedSizePipelineFilter(int size)
        {
            _size = size;
        }
        
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (reader.Length < _size)
                return null;

            var pack = reader.Sequence.Slice(0, _size);

            if (!CanDecodePackage(pack))
                return null;

            reader.Advance(_size);
            return DecodePackage(pack);
        }

        protected virtual bool CanDecodePackage(ReadOnlySequence<byte> buffer)
        {
            return true;
        }

        protected void ResetSize(int size)
        {
            _size = size;
        }
    }
}