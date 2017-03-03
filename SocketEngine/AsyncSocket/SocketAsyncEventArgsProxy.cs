using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Sockets;

namespace SuperSocket.SocketEngine.AsyncSocket
{
    class SocketAsyncEventArgsProxy
    {
        public ISocketAsyncEventArgs SocketEventArgs { get; private set; }

        public int OrigOffset { get; private set; }

        public bool IsRecyclable { get; private set; }

        private SocketAsyncEventArgsProxy()
        {

        }

        public SocketAsyncEventArgsProxy(ISocketAsyncEventArgs socketEventArgs)
            : this(socketEventArgs, true)
        {
            
        }

        public SocketAsyncEventArgsProxy(ISocketAsyncEventArgs socketEventArgs, bool isRecyclable)
        {
            SocketEventArgs = socketEventArgs;
            OrigOffset = socketEventArgs.Offset;
            SocketEventArgs.Completed += new EventHandler<ISocketAsyncEventArgs>(SocketEventArgs_Completed);
            IsRecyclable = isRecyclable;
        }

        static void SocketEventArgs_Completed(object sender, ISocketAsyncEventArgs e)
        {
            var socketSession = e.UserToken as IAsyncSocketSession;

            if (socketSession == null)
                return;

            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                socketSession.AsyncRun(() => socketSession.ProcessReceive(e));
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
