using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public abstract class ClientSession : IClientSession
    {
        protected Socket Client { get; set; }

        protected EndPoint RemoteEndPoint { get; set; }

        public ClientSession()
        {

        }

        public ClientSession(EndPoint remoteEndPoint)
        {
            if (remoteEndPoint == null)
                throw new ArgumentNullException("remoteEndPoint");

            RemoteEndPoint = remoteEndPoint;
        }

        void IClientSession.Connect()
        {
            Connect();
        }

        protected abstract void Connect();

        public abstract void Send(byte[] data, int offset, int length);

        public abstract void Send(IList<ArraySegment<byte>> segments);

        public abstract void Close();

        private EventHandler m_Closed;

        public event EventHandler Closed
        {
            add { m_Closed += value; }
            remove { m_Closed -= value; }
        }

        protected virtual void OnClosed()
        {
            var handler = m_Closed;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private EventHandler<ErrorEventArgs> m_Error;

        public event EventHandler<ErrorEventArgs> Error
        {
            add { m_Error += value; }
            remove { m_Error -= value; }
        }

        protected virtual void OnError(Exception e)
        {
            if (m_Error == null)
                return;

            m_Error(this, new ErrorEventArgs(e));
        }
    }
}
