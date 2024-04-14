using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    public class SerialPackageHandlingScheduler<TPackageInfo> : PackageHandlingSchedulerBase<TPackageInfo>
    {
        public override async ValueTask HandlePackage(IAppSession session, TPackageInfo package, CancellationToken cancellationToken)
        {
            await HandlePackageInternal(session, package, cancellationToken);
        }
    }
}