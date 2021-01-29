using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IPackageHandlingScheduler<TPackageInfo>
    {
        void Initialize(IPackageHandler<TPackageInfo> packageHandler, Func<IAppSession, PackageHandlingException<TPackageInfo>, ValueTask<bool>> errorHandler);

        ValueTask HandlePackage(IAppSession session, TPackageInfo package);
    }
}