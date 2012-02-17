using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System.Threading;
using System.Net.Sockets;
using SuperSocket.Common;

namespace SuperSocket.SocketEngine
{
    class AsyncStreamSocketServer : TcpSocketServerBase
    {
        public AsyncStreamSocketServer(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {
            
        }

        private int m_CurrentConnectionCount;

        public override bool Start()
        {
            try
            {
                if (!base.Start())
                    return false;

                IsRunning = true;
                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                return false;
            }
        }

        protected override void AcceptNewClient(ISocketListener listener, Socket client)
        {
            if(Interlocked.Increment(ref m_CurrentConnectionCount) > AppServer.Config.MaxConnectionNumber)
            {
                Interlocked.Decrement(ref m_CurrentConnectionCount);
                Async.Run(() => client.SafeCloseClientSocket(AppServer.Logger));
                return;
            }

            var session = RegisterSession(client, new AsyncStreamSocketSession(client));

            if (session == null)
            {
                Interlocked.Decrement(ref m_CurrentConnectionCount);
                Async.Run(() => client.SafeCloseClientSocket(AppServer.Logger));
                return;
            }

            session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
            Async.Run(() => session.Start());
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            Interlocked.Decrement(ref m_CurrentConnectionCount);
        }

        public override void Stop()
        {
            if (IsStopped)
                return;

            lock (SyncRoot)
            {
                if (IsStopped)
                    return;

                base.Stop();
                IsRunning = false;
            }
        }
    }
}
