using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperWebSocket
{
    public class WebSocketServer : AppServer<WebSocketSession>
    {
        public WebSocketServer()
            : base()
        {
            Protocol = new WebSocketProtocol();
        }
    }
}
