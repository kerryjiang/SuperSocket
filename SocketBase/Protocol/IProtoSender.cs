using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    interface IProtoSender
    {
        void Send(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> data);

        void Send(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments);

        bool TrySend(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> data);

        bool TrySend(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments);
    }
}
