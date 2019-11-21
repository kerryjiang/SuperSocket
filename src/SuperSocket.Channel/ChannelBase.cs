using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public abstract class ChannelBase<TPackageInfo> : IChannel<TPackageInfo>, IChannel
        where TPackageInfo : class
    {
        public abstract IAsyncEnumerable<TPackageInfo> RunAsync();

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer);

        public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);
        
        public bool IsClosed { get; private set; }

        protected virtual void OnClosed()
        {
            IsClosed = true;
        }

        public abstract void Close();
    }
}
