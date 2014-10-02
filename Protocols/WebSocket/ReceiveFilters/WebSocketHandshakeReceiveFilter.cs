using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    class WebSocketHandshakeReceiveFilter : HttpHeaderReceiveFilterBase<StringPackageInfo>
    {
        private const string RFC6455_VERSION = "13";
        private const string HYBI10_VERSION = "8";

        protected override IReceiveFilter<StringPackageInfo> GetBodyReceiveFilter(HttpHeaderInfo header, int headerSize)
        {
            var session = AppContext.CurrentSession;
            var websocketContext = new WebSocketContext(session, header);

            var secWebSocketVersion = header.Get(WebSocketConstant.SecWebSocketVersion);

            IReceiveFilter<StringPackageInfo> handshakeReceiveFilter = null;

            if (secWebSocketVersion == RFC6455_VERSION)
                handshakeReceiveFilter = new Rfc6455ReceiveFilter();
            else if (secWebSocketVersion == HYBI10_VERSION)
                handshakeReceiveFilter = new DraftHybi10ReceiveFilter();
            else
                handshakeReceiveFilter = new DraftHybi00ReceiveFilter();

            var handshakeHandler = handshakeReceiveFilter as IHandshakeHandler;
            if (handshakeHandler != null)
                handshakeHandler.Handshake(session, header);

            return handshakeReceiveFilter;
        }

        protected override StringPackageInfo ResolveHttpPackageWithoutBody(HttpHeaderInfo header)
        {
            throw new NotSupportedException();
        }
    }
}
