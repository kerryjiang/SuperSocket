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

internal sealed class TcpIocpChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
{
    private int _settingsIndex;
    private readonly QueueSettings[] _settings;
    private readonly int _settingsCount = Environment.ProcessorCount;

    public TcpIocpChannelCreatorFactory(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        const int maxReadBufferSize = 1048576;
        const int maxWriteBufferSize = 65536;

        var applicationScheduler = PipeScheduler.ThreadPool;

        // Socket callbacks run on the threads polling for IO if we're using the old Windows thread pool
        var dispatchSocketCallbacks = OperatingSystem.IsWindows() &&
                                      (Environment.GetEnvironmentVariable("DOTNET_ThreadPool_UsePortableThreadPoolForIO") == "0" ||
                                      Environment.GetEnvironmentVariable("COMPlus_ThreadPool_UsePortableThreadPoolForIO") == "0");

        PipeScheduler SelectSocketsScheduler(PipeScheduler dispatchingScheduler) =>
    dispatchSocketCallbacks ? dispatchingScheduler : PipeScheduler.Inline;

        if (_settingsCount > 0)
        {
            _settings = new QueueSettings[_settingsCount];

            for (var i = 0; i < _settingsCount; i++)
            {
                var memoryPool = new PinnedBlockMemoryPool();
                var transportScheduler = new IOQueue();
                var socketsScheduler = SelectSocketsScheduler(transportScheduler);

                _settings[i] = new QueueSettings()
                {
                    Scheduler = transportScheduler,
                    InputOptions = new PipeOptions(memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false),
                    OutputOptions = new PipeOptions(memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false),
                    SocketSenderPool = new SocketSenderPool(socketsScheduler),
                    MemoryPool = memoryPool,
                };
            }
        }
        else
        {
            var memoryPool = new PinnedBlockMemoryPool();
            var transportScheduler = PipeScheduler.ThreadPool;
            var socketsScheduler = SelectSocketsScheduler(transportScheduler);

            _settings = new QueueSettings[]
            {
                new QueueSettings()
                {
                    Scheduler = transportScheduler,
                    InputOptions = new PipeOptions(memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false),
                    OutputOptions = new PipeOptions(memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false),
                    SocketSenderPool = new SocketSenderPool(socketsScheduler),
                    MemoryPool = memoryPool,
                }
            };
            _settingsCount = 1;
        }
    }

    public new IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options,
                                                                  ChannelOptions channelOptions,
                                                                  ILoggerFactory loggerFactory,
                                                                  object pipelineFilterFactory)
    {
        var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;

        ArgumentNullException.ThrowIfNull(filterFactory);

        channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

        var channelFactoryLogger = loggerFactory.CreateLogger(nameof(TcpChannelCreator));

        return new TcpChannelCreator(options, (s) =>
        {
            QueueSettings setting = _settings[Interlocked.Increment(ref _settingsIndex) % (long)_settingsCount];

            var newChannelOptions = new ChannelOptions
            {
                SendBufferSize = channelOptions.SendBufferSize,
                SendTimeout = channelOptions.SendTimeout,
                ReceiveBufferSize = channelOptions.ReceiveBufferSize,
                Logger = loggerFactory.CreateLogger(nameof(IOCPTcpPipeChannel<TPackageInfo>)),
                MaxPackageLength = channelOptions.MaxPackageLength,
                ReadAsDemand = channelOptions.ReadAsDemand,
                ReceiveTimeout = channelOptions.ReceiveTimeout,
                Values = channelOptions.Values,
                In = new Pipe(setting.InputOptions),
                Out = new Pipe(setting.OutputOptions),
            };

            ApplySocketOptions(s, options, channelOptions, channelFactoryLogger);

            var pipelineFilter = filterFactory.Create(s);

            var channel = new IOCPTcpPipeChannel<TPackageInfo>(socket: s,
                                                               pipelineFilter: pipelineFilter,
                                                               options: newChannelOptions,
                                                               socketSenderPool: setting.SocketSenderPool,
                                                               socketScheduler: setting.SocketSenderPool.Scheduler);

            return ValueTask.FromResult<IChannel>(channel);
        }, channelFactoryLogger);
    }
}