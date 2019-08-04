using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IPackageHandler<TReceivePackageInfo>
        where TReceivePackageInfo : class
    {
        Task Handle(IAppSession session, TReceivePackageInfo package);
    }
}