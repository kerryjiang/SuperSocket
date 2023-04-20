using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.IOCPTcpChannel;
using SuperSocket.Kestrel.IOCP;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.IOCPTcpChannelCreatorFactory;

public class TcpIocpChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
{
    private int _settingsIndex;
    private readonly QueueSettings[] _settings;
    private readonly int _settingsCount = Environment.ProcessorCount;

    public TcpIocpChannelCreatorFactory(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _settings = LoadQueueSettings(serviceProvider);
        _settingsCount = _settings.Length;
    }

    protected virtual QueueSettings[] LoadQueueSettings(IServiceProvider serviceProvider)
    {
        return QueueSettings.Default;
    }

    public new IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options,
                                                                  ChannelOptions channelOptions,
                                                                  ILoggerFactory loggerFactory,
                                                                  object pipelineFilterFactory)
    {
        var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;

        ArgumentNullException.ThrowIfNull(filterFactory);

        channelOptions.Logger = loggerFactory.CreateLogger<IOCPTcpPipeChannel<TPackageInfo>>();

        var channelFactoryLogger = loggerFactory.CreateLogger(nameof(TcpChannelCreator));

        return new TcpChannelCreator(options, (System.Net.Sockets.Socket socket) =>
        {
            QueueSettings setting = _settings[Interlocked.Increment(ref _settingsIndex) % (long)_settingsCount];

            var newChannelOptions = new ChannelOptions
            {
                SendBufferSize = channelOptions.SendBufferSize,
                SendTimeout = channelOptions.SendTimeout,
                ReceiveBufferSize = channelOptions.ReceiveBufferSize,
                Logger = channelOptions.Logger,
                MaxPackageLength = channelOptions.MaxPackageLength,
                ReadAsDemand = channelOptions.ReadAsDemand,
                ReceiveTimeout = channelOptions.ReceiveTimeout,
                Values = channelOptions.Values,
                In = new Pipe(setting.InputOptions),
                Out = new Pipe(setting.OutputOptions),
            };

            ApplySocketOptions(socket, options, channelOptions, channelFactoryLogger);

            var pipelineFilter = filterFactory.Create(socket);

            var channel = new IOCPTcpPipeChannel<TPackageInfo>(socket: socket,
                                                               pipelineFilter: pipelineFilter,
                                                               options: newChannelOptions,
                                                               socketSenderPool: setting.SocketSenderPool,
                                                               socketScheduler: setting.SocketSenderPool.Scheduler);

            return ValueTask.FromResult<IChannel>(channel);
        }, channelFactoryLogger);
    }
}