using Microsoft.Extensions.Logging;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IListenerFactory
    {
        IListener CreateListener<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
            where TPackageInfo : class;
    }
}