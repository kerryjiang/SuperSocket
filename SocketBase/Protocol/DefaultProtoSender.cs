using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    class DefaultProtoSender : IProtoSender
    {
        private readonly int m_SendTimeOut;

        public DefaultProtoSender(int sendTimeOut)
        {
            m_SendTimeOut = sendTimeOut;
        }

        protected bool InternalTrySend(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments)
        {
            if (!session.TrySend(segments))
                return false;

            return true;
        }

        protected void InternalSend(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments)
        {
            if (!session.CanSend() || (protoHandler != null && !protoHandler.CanSend()))
                return;

            if (InternalTrySend(session, protoHandler, segments))
                return;

            var sendTimeOut = m_SendTimeOut;

            //Don't retry, timeout directly
            if (sendTimeOut < 0)
            {
                throw new TimeoutException("The sending attempt timed out");
            }

            var timeOutTime = sendTimeOut > 0 ? DateTime.Now.AddMilliseconds(sendTimeOut) : DateTime.Now;

            var spinWait = new SpinWait();

            while (session.CanSend())
            {
                spinWait.SpinOnce();

                if (InternalTrySend(session, protoHandler, segments))
                    return;

                //If sendTimeOut = 0, don't have timeout check
                if (sendTimeOut > 0 && DateTime.Now >= timeOutTime)
                {
                    throw new TimeoutException("The sending attempt timed out");
                }
            }
        }

        protected bool InternalTrySend(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> segment)
        {
            if (!session.TrySend(segment))
                return false;

            return true;
        }

        protected void InternalSend(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> segment)
        {
            if (!session.CanSend() || (protoHandler != null && !protoHandler.CanSend()))
                return;

            if (InternalTrySend(session, protoHandler, segment))
                return;

            var sendTimeOut = m_SendTimeOut;

            //Don't retry, timeout directly
            if (sendTimeOut < 0)
            {
                throw new TimeoutException("The sending attempt timed out");
            }

            var timeOutTime = sendTimeOut > 0 ? DateTime.Now.AddMilliseconds(sendTimeOut) : DateTime.Now;

            var spinWait = new SpinWait();

            while (session.CanSend())
            {
                spinWait.SpinOnce();

                if (InternalTrySend(session, protoHandler, segment))
                    return;

                //If sendTimeOut = 0, don't have timeout check
                if (sendTimeOut > 0 && DateTime.Now >= timeOutTime)
                {
                    throw new TimeoutException("The sending attempt timed out");
                }
            }
        }

        public virtual void Send(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments)
        {
            InternalSend(session, protoHandler, segments);
        }

        public virtual void Send(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> data)
        {
            InternalSend(session, protoHandler, new ArraySegment<byte>[] { data });
        }

        public virtual bool TrySend(ISocketSession session, IProtoHandler protoHandler, IList<ArraySegment<byte>> segments)
        {
            if (!session.CanSend() || (protoHandler != null && !protoHandler.CanSend()))
                return false;

            return InternalTrySend(session, protoHandler, segments);
        }

        public virtual bool TrySend(ISocketSession session, IProtoHandler protoHandler, ArraySegment<byte> data)
        {
            if (!session.CanSend() || (protoHandler != null && !protoHandler.CanSend()))
                return false;

            return InternalTrySend(session, protoHandler, data);
        }
    }
}
