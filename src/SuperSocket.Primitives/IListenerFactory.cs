namespace SuperSocket
{
    public interface IListenerFactory
    {
        IListener CreateListener<TPackageInfo>(ListenOptions options, object pipelineFilterFactory)
            where TPackageInfo : class;
    }
}