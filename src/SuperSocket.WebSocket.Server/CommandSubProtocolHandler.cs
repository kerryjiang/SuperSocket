using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

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

        public override async ValueTask Handle(IAppSession session, WebSocketPackage package)
        {
            await _commandMiddleware.Handle(session, package);
        }
    }
}