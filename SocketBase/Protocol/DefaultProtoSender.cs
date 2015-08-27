using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    class DefaultProtoSender : IProtoSender
    {
        public void Send(ISocketSession session, IList<ArraySegment<byte>> segments)
        {
            throw new NotImplementedException();
        }

        public void Send(ISocketSession session, ArraySegment<byte> data)
        {
            throw new NotImplementedException();
        }

        public bool TrySend(ISocketSession session, IList<ArraySegment<byte>> segments)
        {
            throw new NotImplementedException();
        }

        public bool TrySend(ISocketSession session, ArraySegment<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
