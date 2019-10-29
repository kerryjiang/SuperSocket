using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class BeginEndMarkPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        private readonly ReadOnlyMemory<byte> _beginMark;

        private readonly ReadOnlyMemory<byte> _endMark;

        private bool _foundBeginMark;
        
        protected BeginEndMarkPipelineFilter(ReadOnlyMemory<byte> beginMark, ReadOnlyMemory<byte> endMark)
        {
            _beginMark = beginMark;
            _endMark = endMark;
        }
        
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!_foundBeginMark)
            {
                var beginMark = _beginMark.Span;

                for (var i = 0; i < beginMark.Length - 1; i++)
                {
                    if (!reader.IsNext(beginMark, advancePast: true))
                    {
                        throw new ProtocolException("Invalid beginning part of the package.");
                    }
                }

                _foundBeginMark = true;
            }

            var endMark =  _endMark.Span;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, endMark, advancePastDelimiter: false))
            {
                return null;
            }

            reader.Advance(endMark.Length);
            return DecodePackage(pack);
        }

        public override void Reset()
        {
            _foundBeginMark = false;
        }
    }
}