using System;
using System.Threading.Tasks;
using SuperSocket;

namespace SuperSocket.Channel
{
    public interface IChannel : IChannelBase
    {
        Task ProcessRequest();
    }

    public interface IChannel<TPackageInfo> : IChannel
        where TPackageInfo : class
    {
        event Action<IChannel, TPackageInfo> PackageReceived;
    }
}
