using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.SocketServiceCore.AsyncSocket
{
    public class SocketAsyncEventArgsProxy
    {
        public SocketAsyncEventArgs ReceiveEventArgs { get; private set; }

        public SocketAsyncEventArgs SendEventArgs { get; private set; }

        private SocketAsyncEventArgsProxy()
        {

        }

        public SocketAsyncEventArgsProxy(SocketAsyncEventArgs receiveEventArgs, SocketAsyncEventArgs sendEventArgs)
        {
            ReceiveEventArgs = receiveEventArgs;
            SendEventArgs = sendEventArgs;
            ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveEventArgs_Completed);
            SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SendEventArgs_Completed);
        }

        static void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Send)
                throw new ArgumentException("The last operation completed on the socket was not a send");

            var token = e.UserToken as AsyncUserToken;
            var socketSession = token.SocketSession;
            socketSession.ProcessSend(e);
        }

        static void ReceiveEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive)
                throw new ArgumentException("The last operation completed on the socket was not a send");

            var token = e.UserToken as AsyncUserToken;
            var socketSession = token.SocketSession;
            socketSession.ProcessReceive(e);
        }

        public Socket Socket
        {
            set
            {
                ((AsyncUserToken)ReceiveEventArgs.UserToken).Socket = value;
                ((AsyncUserToken)SendEventArgs.UserToken).Socket = value;
            }
        }

        public void Initialize(Socket socket, IAsyncSocketSession socketSession, SocketContext socketContext)
        {
            var token = ReceiveEventArgs.UserToken as AsyncUserToken;
            token.Socket = socket;
            token.SocketSession = socketSession;
            token.SocketContext = socketContext;

            token = SendEventArgs.UserToken as AsyncUserToken;
            token.Socket = socket;
            token.SocketSession = socketSession;
            token.SocketContext = socketContext;
        }

        public void Reset()
        {
            var token = ReceiveEventArgs.UserToken as AsyncUserToken;
            token.Socket = null;
            token.SocketSession = null;
            token.SocketContext = null;

            token = SendEventArgs.UserToken as AsyncUserToken;
            token.Socket = null;
            token.SocketSession = null;
            token.SocketContext = null;
        }
    }
}
