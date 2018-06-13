using System;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public interface IChannel
    {
        Task ProcessRequest();

        Task SendAsync(ReadOnlySpan<byte> data);

        event EventHandler Closed;
    }

    public interface IChannel<TPackageInfo> : IChannel
        where TPackageInfo : class
    {
        event Action<IChannel, TPackageInfo> PackageReceived;
    }
}
