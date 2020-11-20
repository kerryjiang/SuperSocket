using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SwitchPipelineFilter
{
    public class SwitchPipelineFilter : PipelineFilterBase<TextPackageInfo>
    {
        private IPipelineFilter<TextPackageInfo> _filterA;
        private byte _beginMarkA = (byte)'Y';

        private IPipelineFilter<TextPackageInfo> _filterB;
        private byte _beginMarkB = (byte)'*';

        public SwitchPipelineFilter()
        {
            _filterA = new PipelineFilterA(this);
            _filterB = new PipelineFilterB(this);
        }

        public override TextPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!reader.TryRead(out byte flag))
                throw new ProtocolException("Flag byte cannot be read.");

            if (flag == _beginMarkA)
                NextFilter = _filterA;
            else if (flag == _beginMarkB)
                NextFilter = _filterB;
            else
                throw new ProtocolException($"Unknown flag at the first postion: {flag}.");

            reader.Rewind(1);
            return null;
        }
    }
}
