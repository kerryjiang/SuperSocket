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
            var socketSession = e.UserToken as IAsyncSocketSession;

            if (socketSession == null)
                return;

            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                Async.Run(() => socketSession.ProcessReceive(e),
                    exc => socketSession.Logger.Error(socketSession as ISessionBase, exc));
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a receive");
            }
        } 

        public void Initialize(IAsyncSocketSession socketSession)
        {
            SocketEventArgs.UserToken = socketSession;
        }

        public void Reset()
        {
            SocketEventArgs.UserToken = null;
        }
    }
}
