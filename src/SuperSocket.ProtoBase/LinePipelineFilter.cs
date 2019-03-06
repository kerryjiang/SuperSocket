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
