using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    class EncodeProtoSender : DefaultProtoSender
    {
        public EncodeProtoSender(int sendTimeOut)
            : base(sendTimeOut)
        {
            
        }

        public override void Send(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> data)
        {
            base.Send(session, protoHandler, protoHandler.DataEncoder.EncodeData(data));
        }

        public override void Send(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments)
        {
            base.Send(session, protoHandler, protoHandler.DataEncoder.EncodeData(segments));
        }

        public override bool TrySend(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> data)
        {
            return base.TrySend(session, protoHandler, protoHandler.DataEncoder.EncodeData(data));
        }

        public override bool TrySend(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments)
        {
            return base.TrySend(session, protoHandler, protoHandler.DataEncoder.EncodeData(segments));
        }
    }
}
