using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine.AsyncSocket
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
                Async.Run(() => socketSession.ProcessReceive(e),
                    exc => socketSession.Logger.LogError(socketSession as ISessionBase, exc));
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a receive");
            }
        } 

        public void Initialize(Socket socket, IAsyncSocketSession socketSession)
        {
            var token = SocketEventArgs.UserToken as AsyncUserToken;
            token.Socket = socket;
            token.SocketSession = socketSession;
        }

        public void Reset()
        {
            var token = SocketEventArgs.UserToken as AsyncUserToken;
            token.Socket = null;
            token.SocketSession = null;
        }
    }
}
