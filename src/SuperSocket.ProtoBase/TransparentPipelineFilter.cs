using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class TransparentPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo>
        where TPackageInfo : class
    {
        public override TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var sequence = reader.Sequence;
            var total = reader.Remaining;
            var package = DecodePackage(ref sequence);
            reader.Advance(total);
            return package;
        }
    }
}
