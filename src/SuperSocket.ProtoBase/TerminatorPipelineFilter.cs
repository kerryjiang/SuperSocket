using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class TerminatorPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private readonly ReadOnlyMemory<byte> _terminator;

        protected TerminatorPipelineFilter(ReadOnlyMemory<byte> terminator)
        {
            _terminator = terminator;
        }
        
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var terminator =  _terminator;
            var terminatorSpan = terminator.Span;

            if (!reader.TryReadToAny(out ReadOnlySequence<byte> pack, terminatorSpan, advancePastDelimiter: false))
            {
                return null;
            }

            for (var i = 0; i < _terminator.Length - 1; i++)
            {
                if (!reader.IsNext(terminatorSpan, advancePast: true))
                {
                    return null;
                }
            }

            return DecodePackage(pack);
        }
    }
}