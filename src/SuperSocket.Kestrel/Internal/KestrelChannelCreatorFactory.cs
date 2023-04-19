using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.Kestrel.Channel;
using SuperSocket.ProtoBase;
using System.Threading.Tasks;

namespace SuperSocket.Kestrel.Internal;

internal sealed class KestrelChannelCreatorFactory : IChannelCreatorFactory
{
    private readonly IKestrelChannelCreator _creator;

    public KestrelChannelCreatorFactory(IKestrelChannelCreator creator)
    {
        _creator = creator;
    }

    IChannelCreator IChannelCreatorFactory.CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
    {
        var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;
        channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

        _creator.ChannelFactoryAsync += (context) =>
        {
            var filter = filterFactory.Create(context);

            var channel = new KestrelPipeChannel<TPackageInfo>(context, filter, channelOptions);

            return new ValueTask<IChannel>(channel);
        };

        return _creator;
    }
}
