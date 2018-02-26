using System;
using System.Buffers;
using System.IO.Pipelines.Text.Primitives;

namespace SuperSocket.ProtoBase
{
    public class LinePipelineFilter : TerminatorPipelineFilter<LinePackageInfo>
    {

        public LinePipelineFilter()
            : base(new byte[] { (byte)'\r', (byte)'\n' })
        {

        }

        public override LinePackageInfo ResolvePackage(ReadOnlyBuffer<byte> buffer)
        {
            return new LinePackageInfo { Line = buffer.GetUtf8Span() };
        }
    }
}
