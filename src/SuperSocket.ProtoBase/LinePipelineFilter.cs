using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class LinePipelineFilter : TerminatorPipelineFilter<LinePackageInfo>
    {

        public LinePipelineFilter()
            : base(new byte[] { (byte)'\r', (byte)'\n' })
        {

        }

        public override LinePackageInfo ResolvePackage(ReadOnlySequence<byte> buffer)
        {
            return new LinePackageInfo { Line = buffer.GetUtf8String() };
        }
    }
}
