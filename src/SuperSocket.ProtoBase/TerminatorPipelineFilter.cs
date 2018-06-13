using System;
using System.Buffers;
using System.Collections;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    public abstract class TerminatorPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private byte[] _terminator;

        public TerminatorPipelineFilter(byte[] terminator)
        {
            _terminator = terminator;
        }
        
        public override TPackageInfo Filter(ref ReadOnlySequence<byte> buffer)
        {
            ReadOnlySequence<byte> slice;
            SequencePosition cursor;

            if (!buffer.TrySliceTo(new Span<byte>(_terminator), out slice, out cursor))
            {
                return null;
            }

            buffer = buffer.Slice(cursor).Slice(_terminator.Length);
            return ResolvePackage(slice);
        }

        public abstract TPackageInfo ResolvePackage(ReadOnlySequence<byte> buffer);
    }
}