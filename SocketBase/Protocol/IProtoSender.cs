using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    interface IProtoSender
    {
        void Send(ISocketSession session, ArraySegment<byte> data);

        void Send(ISocketSession session, IList<ArraySegment<byte>> segments);

        bool TrySend(ISocketSession session, ArraySegment<byte> data);

        bool TrySend(ISocketSession session, IList<ArraySegment<byte>> segments);
    }
}
