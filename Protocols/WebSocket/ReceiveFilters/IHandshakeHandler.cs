using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    interface IHandshakeHandler
    {
        bool Handshake(WebSocketContext context);
    }
}
