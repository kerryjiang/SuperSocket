using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class LinePipelineFilter : TerminatorPipelineFilter<TextPackageInfo>
    {

        public LinePipelineFilter()
            : base(new[] { (byte)'\r', (byte)'\n' })
        {

        }

        protected override TextPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
        {
            return new TextPackageInfo { Text = buffer.GetString(Encoding.UTF8) };
        }
    }
}
