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
            reader.Advance(_size);

            return ResolvePackage(pack);
        }

        public abstract TPackageInfo ResolvePackage(ReadOnlySequence<byte> buffer);
    }
}