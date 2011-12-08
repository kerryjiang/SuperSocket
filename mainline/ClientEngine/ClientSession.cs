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
    }
}
