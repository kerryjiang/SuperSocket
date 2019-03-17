using System;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public interface IChannel
    {
        Task StartAsync();

        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

        event EventHandler Closed;
    }

    public interface IChannel<out TPackageInfo> : IChannel
        where TPackageInfo : class
    {
        event Action<IChannel, TPackageInfo> PackageReceived;
    }
}
