using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IChannel
    {
        Task ProcessRequest();

        Task<int> SendAsync(ReadOnlyMemory<byte> data);

        event EventHandler Closed;
    }

    public interface IChannel<out TPackageInfo> : IChannel
        where TPackageInfo : class
    {
        event Action<IChannel, TPackageInfo> PackageReceived;
    }
}
