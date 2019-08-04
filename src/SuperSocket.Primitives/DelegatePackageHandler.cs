using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class DelegatePackageHandler<TReceivePackageInfo> : IPackageHandler<TReceivePackageInfo>
        where TReceivePackageInfo : class
    {

        Func<IAppSession, TReceivePackageInfo, Task> _func;

        public DelegatePackageHandler(Func<IAppSession, TReceivePackageInfo, Task> func)
        {
            _func = func;
        }

        public async Task Handle(IAppSession session, TReceivePackageInfo package)
        {
            await _func(session, package);
        }
    }
}