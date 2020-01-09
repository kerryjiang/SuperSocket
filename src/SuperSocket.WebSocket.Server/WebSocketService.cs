using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketService : SuperSocketService<WebSocketPackage>
    {
        public WebSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, ILoggerFactory loggerFactory, IChannelCreatorFactory channelCreatorFactory)
            : base(serviceProvider, serverOptions, loggerFactory, channelCreatorFactory)
        {

        }

        internal ValueTask OnSessionHandshakeCompleted(WebSocketSession session)
        {
            return base.FireSessionConnectedEvent(session);
        }

        protected override async ValueTask FireSessionConnectedEvent(AppSession session)
        {
            var websocketSession = session as WebSocketSession;

            if (websocketSession.Handshaked)
                await base.FireSessionConnectedEvent(session);
        }

        protected override async ValueTask FireSessionClosedEvent(AppSession session)
        {
            var websocketSession = session as WebSocketSession;

            if (websocketSession.Handshaked)
                await base.FireSessionClosedEvent(session);
        }
    }
}