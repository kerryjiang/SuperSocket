using System;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IChannel
    {
        Task StartAsync();

        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

        event EventHandler Closed;

        void Close();
    }

    public interface IChannel<out TPackageInfo> : IChannel
        where TPackageInfo : class
    {
        event Func<IChannel, TPackageInfo, Task> PackageReceived;
    }
}
