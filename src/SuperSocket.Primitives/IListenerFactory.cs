using Microsoft.Extensions.Logging;

namespace SuperSocket
{
    public interface IListenerFactory
    {
        IListener CreateListener<TPackageInfo>(ListenOptions options, ILoggerFactory loggerFactory, object pipelineFilterFactory)
            where TPackageInfo : class;
    }
}