using SuperSocket.Kestrel.IOCP;
using System;
using System.Buffers;
using System.IO.Pipelines;

namespace SuperSocket.IOCPTcpChannelCreatorFactory;

public sealed class QueueSettings
{
    public static int IOQueueCount => Environment.ProcessorCount;

    public PipeScheduler Scheduler { get; set; } = null!;

    public PipeOptions InputOptions { get; set; } = null!;

    public PipeOptions OutputOptions { get; set; } = null!;

    public SocketSenderPool SocketSenderPool { get; set; } = null!;

    public MemoryPool<byte> MemoryPool { get; set; } = null!;

    public static QueueSettings[] Default => OnLoadDefault();

    private static QueueSettings[] OnLoadDefault()
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

        QueueSettings[] settings;

        if (IOQueueCount > 0)
        {
            settings = new QueueSettings[IOQueueCount];

            for (var i = 0; i < IOQueueCount; i++)
            {
                var memoryPool = new PinnedBlockMemoryPool();
                var transportScheduler = new IOQueue();
                var socketsScheduler = SelectSocketsScheduler(transportScheduler);

                settings[i] = new QueueSettings()
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

            settings = new QueueSettings[]
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
        }

        return settings;
    }
}

