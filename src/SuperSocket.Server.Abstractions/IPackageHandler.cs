using System;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions
{
    public interface IPackageHandler<TReceivePackageInfo>
    {
        ValueTask Handle(IAppSession session, TReceivePackageInfo package);
    }
}