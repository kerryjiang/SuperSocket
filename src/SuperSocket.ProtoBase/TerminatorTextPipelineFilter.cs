using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class TerminatorTextPipelineFilter : TerminatorPipelineFilter<TextPackageInfo>
    {

        public TerminatorTextPipelineFilter(ReadOnlyMemory<byte> terminator)
            : base(terminator)
        {

        }

        public override TextPackageInfo ResolvePackage(ReadOnlySequence<byte> buffer)
        {
            return new TextPackageInfo { Text = buffer.GetString(Encoding.UTF8) };
        }
    }
}
