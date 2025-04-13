using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// A scheduler for handling packages concurrently.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class ConcurrentPackageHandlingScheduler<TPackageInfo> : PackageHandlingSchedulerBase<TPackageInfo>
    {
        /// <summary>
        /// Handles a package by scheduling it for concurrent processing.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The package to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override ValueTask HandlePackage(IAppSession session, TPackageInfo package, CancellationToken cancellationToken)
        {
            HandlePackageInternal(session, package, cancellationToken).DoNotAwait();
            return new ValueTask();
        }
    }
}