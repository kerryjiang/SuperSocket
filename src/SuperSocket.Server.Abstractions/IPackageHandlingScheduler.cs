using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Defines a scheduler for handling packages.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IPackageHandlingScheduler<TPackageInfo>
    {
        /// <summary>
        /// Initializes the scheduler with a package handler and an error handler.
        /// </summary>
        /// <param name="packageHandler">The handler for processing packages.</param>
        /// <param name="errorHandler">The handler for processing errors during package handling.</param>
        void Initialize(IPackageHandler<TPackageInfo> packageHandler, Func<IAppSession, PackageHandlingException<TPackageInfo>, ValueTask<bool>> errorHandler);

        /// <summary>
        /// Handles a package asynchronously.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The received package.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        ValueTask HandlePackage(IAppSession session, TPackageInfo package, CancellationToken cancellationToken);
    }
}