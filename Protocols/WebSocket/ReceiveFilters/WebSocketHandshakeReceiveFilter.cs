using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    class WebSocketHandshakeReceiveFilter : HttpHeaderReceiveFilterBase<WebSocketPackageInfo>
    {
        private const string RFC6455_VERSION = "13";
        private const string HYBI10_VERSION = "8";

        private WebSocketContext m_Context;

        public WebSocketHandshakeReceiveFilter(WebSocketContext context)
        {
            m_Context = context;
        }

        protected override IReceiveFilter<WebSocketPackageInfo> GetBodyReceiveFilter(HttpHeaderInfo header, int headerSize)
        {
            var websocketContext = m_Context;
            websocketContext.HandshakeRequest = header;

            var handshakeReceiveFilter = WebSocketReceiveFilterFactoryManager.Handshake(websocketContext);
            return handshakeReceiveFilter;
        }

        protected override WebSocketPackageInfo ResolveHttpPackageWithoutBody(HttpHeaderInfo header)
        {
            throw new NotSupportedException();
        }
    }
}
