using System;
using System.IO.Pipelines;
using System.Net.Sockets;
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
        public override TPackageInfo Filter(ref ReadableBuffer buffer)
        {
            ReadableBuffer slice;
            ReadCursor cursor;

            if (!buffer.TrySliceTo(new Span<byte>(_terminator), out slice, out cursor))
            {
                return null;
            }

            buffer = buffer.Slice(cursor).Slice(_terminator.Length);
            return ResolvePackage(slice);
        }

        public abstract TPackageInfo ResolvePackage(ReadableBuffer buffer);
    }
}