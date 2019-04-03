using System;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public abstract class ChannelBase<TPackageInfo> : IChannel<TPackageInfo>, IChannel
        where TPackageInfo : class
    {
        public abstract Task StartAsync();

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer);

        public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

        private Func<IChannel, TPackageInfo, Task> _packageReceived;

        public event Func<IChannel, TPackageInfo, Task> PackageReceived
        {
            add => _packageReceived += value;
            remove => _packageReceived -= value;
        }

        protected async Task OnPackageReceived(TPackageInfo package)
        {
            await _packageReceived?.Invoke(this, package);
        }

        private EventHandler _closed;

        public event EventHandler Closed
        {
            add => _closed += value;
            remove => _closed -= value;
        }

        protected virtual void OnClosed()
        {
            _closed?.Invoke(this, EventArgs.Empty);
        }

        public abstract void Close();
    }
}
