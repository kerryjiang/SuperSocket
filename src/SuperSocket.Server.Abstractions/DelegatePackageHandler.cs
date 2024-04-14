using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions
{
    public class DelegatePackageHandler<TReceivePackageInfo> : IPackageHandler<TReceivePackageInfo>
    {
        Func<IAppSession, TReceivePackageInfo, CancellationToken, ValueTask> _func;

        public DelegatePackageHandler(Func<IAppSession, TReceivePackageInfo, ValueTask> func)
        {
            _func = (session, package, cancellationToken) => func(session, package);
        }

        public DelegatePackageHandler(Func<IAppSession, TReceivePackageInfo, CancellationToken, ValueTask> func)
        {
            _func = func;
        }

        public async ValueTask Handle(IAppSession session, TReceivePackageInfo package, CancellationToken cancellationToken)
        {
            await _func(session, package, cancellationToken);
        }
    }
}