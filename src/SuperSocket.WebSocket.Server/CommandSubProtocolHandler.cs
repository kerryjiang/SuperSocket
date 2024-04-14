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
    sealed class CommandSubProtocolHandler<TPackageInfo> : SubProtocolHandlerBase
    {
        private IPackageHandler<WebSocketPackage> _commandMiddleware;

        public CommandSubProtocolHandler(string name, IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions, IPackageMapper<WebSocketPackage, TPackageInfo> mapper)
            : base(name)
        {
            var keyType = CommandMiddlewareExtensions.GetKeyType<TPackageInfo>();
            var commandMiddlewareType = typeof(WebSocketCommandMiddleware<,>).MakeGenericType(keyType, typeof(TPackageInfo));
            _commandMiddleware = Activator.CreateInstance(commandMiddlewareType, serviceProvider, commandOptions, mapper) as IPackageHandler<WebSocketPackage>;
        }

        public override async ValueTask Handle(IAppSession session, WebSocketPackage package, CancellationToken cancellationToken)
        {
            await _commandMiddleware.Handle(session, package, cancellationToken);
        }
    }
}