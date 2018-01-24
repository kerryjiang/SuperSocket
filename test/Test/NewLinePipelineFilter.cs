namespace Tests
{
    public class NewLinePipelineFilter : TerminatorPipelineFilter<LinePackageInfo>
    {
        public NewLinePipelineFilter()
            : base(new byte[] { (byte)'\r', (byte)'\n' })
        {
        }

        public override LinePackageInfo ResolvePackage(ReadOnlyBuffer<byte> buffer)
        {
            return new LinePackageInfo { Line = buffer.GetUtf8Span() };
        }
    }
}