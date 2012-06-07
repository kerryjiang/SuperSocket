using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.SocketEngine
{
    abstract class SocketListenerBase : ISocketListener
    {
        public ListenerInfo Info { get; private set; }

        public IPEndPoint EndPoint
        {
            get { return Info.EndPoint; }
        }

        protected SocketListenerBase(ListenerInfo info)
        {
            Info = info;
        }

        public abstract bool Start();

        public abstract void Stop();

        public event NewClientAcceptHandler NewClientAccepted;

        public event ErrorHandler Error;

        protected void OnError(Exception e)
        {
            var handler = Error;

            if(handler != null)
                handler(this, e);
        }

        protected void OnError(string errorMessage)
        {
            OnError(new Exception(errorMessage));
        }

        protected virtual void OnNewClientAccepted(Socket socket, object state)
        {
            NewClientAccepted.BeginInvoke(this, socket, state, null, null);
        }
    }
}
