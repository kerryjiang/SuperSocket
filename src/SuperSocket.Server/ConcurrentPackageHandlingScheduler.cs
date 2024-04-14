using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    public class ConcurrentPackageHandlingScheduler<TPackageInfo> : PackageHandlingSchedulerBase<TPackageInfo>
    {
        public override ValueTask HandlePackage(IAppSession session, TPackageInfo package, CancellationToken cancellationToken)
        {
            HandlePackageInternal(session, package, cancellationToken).DoNotAwait();
            return new ValueTask();
        }
    }
}