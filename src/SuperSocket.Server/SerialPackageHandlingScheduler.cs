using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// A package handling scheduler that processes packages serially.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class SerialPackageHandlingScheduler<TPackageInfo> : PackageHandlingSchedulerBase<TPackageInfo>
    {
        /// <summary>
        /// Handles a package for a given session.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The package to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async ValueTask HandlePackage(IAppSession session, TPackageInfo package, CancellationToken cancellationToken)
        {
            await HandlePackageInternal(session, package, cancellationToken);
        }
    }
}