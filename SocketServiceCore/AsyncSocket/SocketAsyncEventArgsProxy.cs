using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.SocketServiceCore.AsyncSocket
{
    public class SocketAsyncEventArgsProxy
    {
        private SocketAsyncEventArgs m_EventArgs;

        public SocketAsyncEventArgs EventArgs
        {
            get { return m_EventArgs; }
        }

        public IAsyncSocketSession SocketSession { get; set; }

        private SocketAsyncEventArgsProxy()
        {

        }

        public SocketAsyncEventArgsProxy(SocketAsyncEventArgs eventArgs)
        {
            m_EventArgs = eventArgs;
            eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
        }

        void eventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    SocketSession.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    SocketSession.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        public Socket Socket
        {
            set
            {
                ((AsyncUserToken)m_EventArgs.UserToken).Socket = value;
            }
        }
    }
}
