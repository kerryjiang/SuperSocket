using System;
using System.Buffers;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketPackageHandler : IPackageHandler<WebSocketPackage>
    {
        private IServiceProvider _serviceProvider;
        private IWebSocketCommandMiddleware _websocketCommandMiddleware;

        public WebSocketPackageHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _websocketCommandMiddleware = serviceProvider
                .GetServices<IMiddleware>()
                .OfType<IWebSocketCommandMiddleware>()
                .FirstOrDefault();
        }

        public async Task Handle(IAppSession session, WebSocketPackage package)
        {
            var websocketSession = session as WebSocketSession;
            
            if (package.OpCode == OpCode.Handshake)
            {
                // handshake failure
                if (!await HandleHandshake(session, package))
                    return;

                websocketSession.Handshaked = true;
                return;
            }


            if (!websocketSession.Handshaked)
            {
                // not pass handshake but receive data package now
                // impossible routine
                return;
            }

            var websocketCommandMiddleware = _websocketCommandMiddleware;

            if (websocketCommandMiddleware != null)
            {
                websocketCommandMiddleware.Register(session.Server as IServer, session);
            }
        }

        private async Task<bool> HandleHandshake(IAppSession session, WebSocketPackage p)
        {
            return await Task.FromResult(true);
        }
    }
}
