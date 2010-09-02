using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.SocketServiceCore.AsyncSocket
{
    class SocketAsyncEventArgsProxy
    {
        public SocketAsyncEventArgs SocketEventArgs { get; private set; }

        private SocketAsyncEventArgsProxy()
        {

        }

        public SocketAsyncEventArgsProxy(SocketAsyncEventArgs socketEventArgs)
        {
            SocketEventArgs = socketEventArgs;
            SocketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArgs_Completed);
        }

        static void SocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            var token = e.UserToken as AsyncUserToken;
            var socketSession = token.SocketSession;

            if (socketSession == null)
                return;            

            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                socketSession.ProcessReceive(e);
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a receive");
            }
        }

        public Socket Socket
        {
            set
            {
                ((AsyncUserToken)SocketEventArgs.UserToken).Socket = value;
            }
        }

        public void Initialize(Socket socket, IAsyncSocketSession socketSession, SocketContext socketContext)
        {
            var token = SocketEventArgs.UserToken as AsyncUserToken;
            token.Socket = socket;
            token.SocketSession = socketSession;
            token.SocketContext = socketContext;
        }

        public void Reset()
        {
            var token = SocketEventArgs.UserToken as AsyncUserToken;
            token.Socket = null;
            token.SocketSession = null;
            token.SocketContext = null;
        }
    }
}
