using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Command
{
    interface ICommandWrap
    {
        ICommand InnerCommand { get; }
    }

    /// <summary>
    /// Represents a wrapper for a command, allowing additional functionality or dependency injection.
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    /// <typeparam name="IPackageInterface">The interface implemented by the package.</typeparam>
    /// <typeparam name="TCommand">The type of the wrapped command.</typeparam>
    class CommandWrap<TAppSession, TPackageInfo, IPackageInterface, TCommand> : ICommand<TAppSession, TPackageInfo>, ICommandWrap
        where TAppSession : IAppSession
        where TPackageInfo : IPackageInterface
        where TCommand : ICommand<TAppSession, IPackageInterface>
    {
        /// <summary>
        /// Gets the inner command wrapped by this instance.
        /// </summary>
        public TCommand InnerCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandWrap{TAppSession, TPackageInfo, IPackageInterface, TCommand}"/> class with the specified command.
        /// </summary>
        /// <param name="command">The command to wrap.</param>
        public CommandWrap(TCommand command)
        {
            InnerCommand = command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandWrap{TAppSession, TPackageInfo, IPackageInterface, TCommand}"/> class using the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public CommandWrap(IServiceProvider serviceProvider)
        {
            InnerCommand = (TCommand)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TCommand));
        }

        /// <summary>
        /// Executes the wrapped command with the specified session and package.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="package">The package information.</param>
        public void Execute(TAppSession session, TPackageInfo package)
        {
            InnerCommand.Execute(session, package);
        }

        ICommand ICommandWrap.InnerCommand => InnerCommand;
    }

    /// <summary>
    /// Represents a wrapper for an asynchronous command, allowing additional functionality or dependency injection.
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    /// <typeparam name="IPackageInterface">The interface implemented by the package.</typeparam>
    /// <typeparam name="TAsyncCommand">The type of the wrapped asynchronous command.</typeparam>
    class AsyncCommandWrap<TAppSession, TPackageInfo, IPackageInterface, TAsyncCommand> : IAsyncCommand<TAppSession, TPackageInfo>, ICommandWrap
        where TAppSession : IAppSession
        where TPackageInfo : IPackageInterface
        where TAsyncCommand : IAsyncCommand<TAppSession, IPackageInterface>
    {
        /// <summary>
        /// Gets the inner asynchronous command wrapped by this instance.
        /// </summary>
        public TAsyncCommand InnerCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommandWrap{TAppSession, TPackageInfo, IPackageInterface, TAsyncCommand}"/> class with the specified asynchronous command.
        /// </summary>
        /// <param name="command">The asynchronous command to wrap.</param>
        public AsyncCommandWrap(TAsyncCommand command)
        {
            InnerCommand = command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommandWrap{TAppSession, TPackageInfo, IPackageInterface, TAsyncCommand}"/> class using the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public AsyncCommandWrap(IServiceProvider serviceProvider)
        {
            InnerCommand = (TAsyncCommand)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TAsyncCommand));
        }

        /// <summary>
        /// Asynchronously executes the wrapped command with the specified session and package.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="package">The package information.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        public async ValueTask ExecuteAsync(TAppSession session, TPackageInfo package, CancellationToken cancellationToken)
        {
            await InnerCommand.ExecuteAsync(session, package, cancellationToken);
        }

        ICommand ICommandWrap.InnerCommand => InnerCommand;
    }
}
