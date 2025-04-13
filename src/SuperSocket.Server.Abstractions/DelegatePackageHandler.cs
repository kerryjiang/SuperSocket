using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents a delegate-based package handler.
    /// </summary>
    /// <typeparam name="TReceivePackageInfo">The type of the package information.</typeparam>
    public class DelegatePackageHandler<TReceivePackageInfo> : IPackageHandler<TReceivePackageInfo>
    {
        private readonly Func<IAppSession, TReceivePackageInfo, CancellationToken, ValueTask> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePackageHandler{TReceivePackageInfo}"/> class with a delegate that does not use a cancellation token.
        /// </summary>
        /// <param name="func">The delegate to handle the package.</param>
        public DelegatePackageHandler(Func<IAppSession, TReceivePackageInfo, ValueTask> func)
        {
            _func = (session, package, cancellationToken) => func(session, package);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePackageHandler{TReceivePackageInfo}"/> class with a delegate that uses a cancellation token.
        /// </summary>
        /// <param name="func">The delegate to handle the package.</param>
        public DelegatePackageHandler(Func<IAppSession, TReceivePackageInfo, CancellationToken, ValueTask> func)
        {
            _func = func;
        }

        /// <summary>
        /// Handles the received package using the provided delegate.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The received package.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        public async ValueTask Handle(IAppSession session, TReceivePackageInfo package, CancellationToken cancellationToken)
        {
            await _func(session, package, cancellationToken);
        }
    }
}