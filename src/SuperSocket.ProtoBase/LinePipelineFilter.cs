namespace SuperSocket.ProtoBase
{
    public class LinePipelineFilter : TerminatorTextPipelineFilter
    {

        public LinePipelineFilter()
            : base(new[] { (byte)'\r', (byte)'\n' })
        {

        }
    }
}
