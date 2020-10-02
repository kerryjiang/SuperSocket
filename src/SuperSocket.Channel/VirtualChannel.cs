using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public abstract class VirtualChannel : IVirtualChannel
    {
        public IChannel Channel { get; private set; }

        public bool IsClosed => Channel.IsClosed;

        public virtual EndPoint RemoteEndPoint => Channel.RemoteEndPoint;

        public virtual EndPoint LocalEndPoint => Channel.LocalEndPoint;

        public DateTimeOffset LastActiveTime { get; protected set; }

        public CloseReason? CloseReason { get; protected set; }

        public VirtualChannel(IChannel channel)
        {
            Channel = channel;
        }

        public event EventHandler<CloseEventArgs> Closed;

        public virtual void Start()
        {
            
        }

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> data);

        public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

        public abstract ValueTask SendAsync(Action<PipeWriter> write);

        public virtual ValueTask CloseAsync(CloseReason closeReason)
        {
            return new ValueTask();
        }

        public virtual ValueTask DetachAsync()
        {
            return new ValueTask();
        }
    }
}
