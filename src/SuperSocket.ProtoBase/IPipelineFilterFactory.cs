namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilterFactory<TPackageInfo>
        where TPackageInfo : class
    {
        IPipelineFilter<TPackageInfo> Create(object client);
    }
}