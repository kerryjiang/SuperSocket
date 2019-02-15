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
        
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var terminator =  new ReadOnlySpan<byte>(_terminator);

            if (!reader.TryReadToAny(out ReadOnlySpan<byte> pack, terminator, advancePastDelimiter:false))
            {
                return null;
            }

            for (var i = 0; i < _terminator.Length - 1; i++)
            {
                if (!reader.IsNext(_terminator, advancePast: true))
                {
                    return null;
                }
            }

            return ResolvePackage(pack);
        }

        public abstract TPackageInfo ResolvePackage(ReadOnlySpan<byte> buffer);
    }
}