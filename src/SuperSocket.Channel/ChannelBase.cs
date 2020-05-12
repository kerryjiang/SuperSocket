using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public abstract class ChannelBase<TPackageInfo> : IChannel<TPackageInfo>, IChannel
    {
        public abstract IAsyncEnumerable<TPackageInfo> RunAsync();

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer);

        public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);
        
        public abstract ValueTask SendAsync(Action<PipeWriter> write);

        public bool IsClosed { get; private set; }

        public EndPoint RemoteEndPoint { get; protected set; }

        public EndPoint LocalEndPoint { get; protected set; }

        public DateTimeOffset LastActiveTime { get; protected set; } = DateTimeOffset.Now;

        protected virtual void OnClosed()
        {
            IsClosed = true;

            var closed = Closed;

            if (closed == null)
                return;

            if (Interlocked.CompareExchange(ref Closed, null, closed) != closed)
                return;

            closed.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Closed;

        public abstract ValueTask CloseAsync();

        public abstract ValueTask DetachAsync();
    }
}
