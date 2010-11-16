using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace NWebSocket
{
    public class NWebSocketServer : AppServer<NWebSocketSession>
    {
        public NWebSocketServer()
            : base()
        {
            Protocol = new NWebSocketProtocol();
        }
    }
}
