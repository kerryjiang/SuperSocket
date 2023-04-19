using SuperSocket.Kestrel.IOCP;
using System.Buffers;
using System.IO.Pipelines;

namespace SuperSocket.IOCPTcpChannelCreatorFactory;

internal sealed class QueueSettings
{
    public PipeScheduler Scheduler { get; set; } = null!;

    public PipeOptions InputOptions { get; set; } = null!;

    public PipeOptions OutputOptions { get; set; } = null!;

    public SocketSenderPool SocketSenderPool { get; set; } = null!;

    public MemoryPool<byte> MemoryPool { get; set; } = null!;
}

