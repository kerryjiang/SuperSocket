using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common;

namespace SuperSocket.WebSocket
{
    public class WebSocketContext
    {
        private const string c_WebSocketContextKey = "WebSocketContext";

        public HttpHeaderInfo HandshakeRequest { get; private set; }

        public string Origin { get; private set; }

        public string UriScheme { get; private set; }

        public string Host { get; private set; }

        public string Path { get; private set; }

        public IAppSession Session { get; private set; }

        public WebSocketContext(IAppSession session, HttpHeaderInfo request)
        {
            Session = session;
            HandshakeRequest = request;
        }

        public string GetAvailableSubProtocol()
        {
            return HandshakeRequest.Get(WebSocketConstant.SecWebSocketProtocol);
        }

        public static WebSocketContext Get(IAppSession session)
        {
            object context;

            if (!session.Items.TryGetValue(c_WebSocketContextKey, out context))
                return null;

            return context as WebSocketContext;
        }
    }
}
