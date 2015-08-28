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
        private IProtoDataEncoder m_Encoder;

        public EncodeProtoSender(int sendTimeOut, IProtoDataEncoder encoder)
            : base(sendTimeOut)
        {
            m_Encoder = encoder;
        }

        public override void Send(ISocketSession session, ArraySegment<byte> data)
        {
            base.Send(session, m_Encoder.EncodeData(data));
        }

        public override void Send(ISocketSession session, IList<ArraySegment<byte>> segments)
        {
            base.Send(session, m_Encoder.EncodeData(segments));
        }

        public override bool TrySend(ISocketSession session, ArraySegment<byte> data)
        {
            return base.TrySend(session, m_Encoder.EncodeData(data));
        }

        public override bool TrySend(ISocketSession session, IList<ArraySegment<byte>> segments)
        {
            return base.TrySend(session, m_Encoder.EncodeData(segments));
        }
    }
}
