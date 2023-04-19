using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.IOCPTcpChannel;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.IOCPTcpChannelCreatorFactory;

internal sealed class TcpIocpChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
{
    public TcpIocpChannelCreatorFactory(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public new IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions,
        ILoggerFactory loggerFactory, object pipelineFilterFactory)
    {
        var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;

        ArgumentNullException.ThrowIfNull(filterFactory);

        channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

        var channelFactoryLogger = loggerFactory.CreateLogger(nameof(TcpChannelCreator));

        return new TcpChannelCreator(options, (s) =>
        {
            ApplySocketOptions(s, options, channelOptions, channelFactoryLogger);
            return new ValueTask<IChannel>(
                (new IOCPTcpPipeChannel<TPackageInfo>(s, filterFactory.Create(s), channelOptions, new SocketSenderPool(PipeScheduler.ThreadPool),PipeScheduler.ThreadPool)));
        }, channelFactoryLogger);
    }
}