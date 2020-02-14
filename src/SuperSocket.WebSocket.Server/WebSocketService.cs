using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketService : SuperSocketService<WebSocketPackage>
    {
        IMiddleware _sessionContainerMiddleware;
        public WebSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, ILoggerFactory loggerFactory, IChannelCreatorFactory channelCreatorFactory)
            : base(serviceProvider, serverOptions, loggerFactory, channelCreatorFactory)
        {
            _sessionContainerMiddleware = Middlewares.FirstOrDefault(m => m is IAsyncSessionContainer || m is ISessionContainer);
        }

        internal async ValueTask OnSessionHandshakeCompleted(WebSocketSession session)
        {
            var sessionContainer = _sessionContainerMiddleware;

            if (sessionContainer != null)
                await sessionContainer.RegisterSession(session);

            await base.FireSessionConnectedEvent(session);
        }
    }
}