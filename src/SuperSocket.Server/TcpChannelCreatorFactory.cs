using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class TcpChannelCreatorFactory : IChannelCreatorFactory
    {
        public IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
            where TPackageInfo : class
        {
            var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;
            channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));
            return new TcpChannelCreator(options, (s) => new TcpPipeChannel<TPackageInfo>(s, filterFactory.Create(s), channelOptions), loggerFactory.CreateLogger(nameof(TcpChannelCreator)));
        }
    }
}