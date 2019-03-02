using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class LinePipelineFilter : TerminatorTextPipelineFilter
    {

        public LinePipelineFilter()
            : base(new byte[] { (byte)'\r', (byte)'\n' })
        {

        }
    }
}
