namespace Tests
{
    public class FakePipelineFilter : IPipelineFilter<FakePackageInfo>
    {
        public IPipelineFilter<FakePackageInfo> NextFilter => throw new NotImplementedException();

        public FakePackageInfo Filter(ref ReadOnlyBuffer<byte> buffer)
        {
            throw new NotImplementedException();
        }
    }
}