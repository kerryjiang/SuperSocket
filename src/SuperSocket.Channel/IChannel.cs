using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IChannel
    {
        void Start();
        
        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

        ValueTask SendAsync(Action<PipeWriter> write);

        ValueTask CloseAsync(CloseReason closeReason);

        event EventHandler<CloseEventArgs> Closed;

        bool IsClosed { get; }

        EndPoint RemoteEndPoint { get; }

        EndPoint LocalEndPoint { get; }

        DateTimeOffset LastActiveTime { get; }

        ValueTask DetachAsync();

        CloseReason? CloseReason { get; }
    }

    public interface IChannel<TPackageInfo> : IChannel
    {
        IAsyncEnumerable<TPackageInfo> RunAsync();
    }
}
