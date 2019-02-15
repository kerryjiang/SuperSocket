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

        public override LinePackageInfo ResolvePackage(ReadOnlySpan<byte> buffer)
        {
            return new LinePackageInfo { Line = Encoding.UTF8.GetString(buffer) };
        }
    }
}
