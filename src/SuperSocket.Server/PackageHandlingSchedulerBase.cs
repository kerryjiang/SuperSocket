using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// Base class for package handling schedulers.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public abstract class PackageHandlingSchedulerBase<TPackageInfo> : IPackageHandlingScheduler<TPackageInfo>
    {
        /// <summary>
        /// Gets the package handler.
        /// </summary>
        public IPackageHandler<TPackageInfo> PackageHandler { get; private set; }

        /// <summary>
        /// Gets the error handler for package handling exceptions.
        /// </summary>
        public Func<IAppSession, PackageHandlingException<TPackageInfo>, ValueTask<bool>> ErrorHandler { get; private set; }

        /// <summary>
        /// Handles a package for a given session.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The package to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract ValueTask HandlePackage(IAppSession session, TPackageInfo package, CancellationToken cancellationToken);

        /// <summary>
        /// Initializes the package handler and error handler.
        /// </summary>
        /// <param name="packageHandler">The package handler to use.</param>
        /// <param name="errorHandler">The error handler to use.</param>
        public virtual void Initialize(IPackageHandler<TPackageInfo> packageHandler, Func<IAppSession, PackageHandlingException<TPackageInfo>, ValueTask<bool>> errorHandler)
        {
            PackageHandler = packageHandler;
            ErrorHandler = errorHandler;
        }

        /// <summary>
        /// Handles a package internally with error handling.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The package to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async ValueTask HandlePackageInternal(IAppSession session, TPackageInfo package, CancellationToken cancellationToken)
        {
            var packageHandler = PackageHandler;
            var errorHandler = ErrorHandler;

            try
            {
                if (packageHandler != null)
                    await packageHandler.Handle(session, package, cancellationToken);
            }
            catch (Exception e)
            {
                var toClose = await errorHandler(session, new PackageHandlingException<TPackageInfo>($"Session {session.SessionID} got an error when handle a package.", package, e));

                if (toClose)
                {
                    session.CloseAsync(CloseReason.ApplicationError).DoNotAwait();
                }
            }
        }
    }
}