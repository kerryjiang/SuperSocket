using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    interface IHandshakeHandler
    {
        void Handshake(IAppSession session, HttpHeaderInfo head);
    }
}
