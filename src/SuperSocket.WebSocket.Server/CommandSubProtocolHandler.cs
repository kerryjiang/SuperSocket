using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SuperSocket.Command;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.WebSocket.Server
{
    /// <summary>
    /// Handles WebSocket sub-protocol commands.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    sealed class CommandSubProtocolHandler<TPackageInfo> : SubProtocolHandlerBase
    {
        private IPackageHandler<WebSocketPackage> _commandMiddleware;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandSubProtocolHandler{TPackageInfo}"/> class.
        /// </summary>
        /// <param name="name">The name of the sub-protocol.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="commandOptions">The command options.</param>
        /// <param name="mapper">The package mapper.</param>
        public CommandSubProtocolHandler(string name, IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions, IPackageMapper<WebSocketPackage, TPackageInfo> mapper)
            : base(name)
        {
            var keyType = CommandMiddlewareExtensions.GetKeyType<TPackageInfo>();
            var commandMiddlewareType = typeof(WebSocketCommandMiddleware<,>).MakeGenericType(keyType, typeof(TPackageInfo));
            _commandMiddleware = Activator.CreateInstance(commandMiddlewareType, serviceProvider, commandOptions, mapper) as IPackageHandler<WebSocketPackage>;
        }

        /// <summary>
        /// Handles a WebSocket package for the sub-protocol.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The WebSocket package.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        public override async ValueTask Handle(IAppSession session, WebSocketPackage package, CancellationToken cancellationToken)
        {
            await _commandMiddleware.Handle(session, package, cancellationToken);
        }
    }
}