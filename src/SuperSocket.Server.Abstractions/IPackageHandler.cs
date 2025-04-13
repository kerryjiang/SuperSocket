using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Defines a handler for processing received packages.
    /// </summary>
    /// <typeparam name="TReceivePackageInfo">The type of the package information.</typeparam>
    public interface IPackageHandler<TReceivePackageInfo>
    {
        /// <summary>
        /// Handles a received package.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The received package.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        ValueTask Handle(IAppSession session, TReceivePackageInfo package, CancellationToken cancellationToken);
    }
}