using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Command
{
    /// <summary>
    /// Represents a base interface for commands.
    /// </summary>
    public interface ICommand
    {
        // empty interface
    }

    /// <summary>
    /// Represents a command that operates on a specific package type.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface ICommand<TPackageInfo> : ICommand<IAppSession, TPackageInfo>
    {
    }

    /// <summary>
    /// Represents a command that operates on a specific session and package type.
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface ICommand<TAppSession, TPackageInfo> : ICommand
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Executes the command with the specified session and package.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="package">The package information.</param>
        void Execute(TAppSession session, TPackageInfo package);
    }

    /// <summary>
    /// Represents an asynchronous command that operates on a specific package type.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IAsyncCommand<TPackageInfo> : IAsyncCommand<IAppSession, TPackageInfo>
    {
    }

    /// <summary>
    /// Represents an asynchronous command that operates on a specific session and package type.
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IAsyncCommand<TAppSession, TPackageInfo> : ICommand
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Asynchronously executes the command with the specified session and package.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="package">The package information.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        ValueTask ExecuteAsync(TAppSession session, TPackageInfo package, CancellationToken cancellationToken);
    }
}
