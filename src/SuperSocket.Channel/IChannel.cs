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
        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

        ValueTask SendAsync(Action<PipeWriter> write);

        void Close();

        bool IsClosed { get; }

        EndPoint RemoteEndPoint { get; }

        EndPoint LocalEndPoint { get; }

        DateTimeOffset LastActiveTime { get; }
    }

    public interface IChannel<TPackageInfo> : IChannel
    {
        IAsyncEnumerable<TPackageInfo> RunAsync();
    }
}
