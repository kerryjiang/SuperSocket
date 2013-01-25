using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    class AsyncSocketServer : TcpSocketServerBase
    {
        public AsyncSocketServer(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {

        }

        private BufferManager m_BufferManager;

        private ConcurrentStack<SocketAsyncEventArgsProxy> m_ReadWritePool;

        public override bool Start()
        {
            try
            {
                int bufferSize = AppServer.Config.ReceiveBufferSize;

                if (bufferSize <= 0)
                    bufferSize = 1024 * 4;

                m_BufferManager = new BufferManager(bufferSize * AppServer.Config.MaxConnectionNumber, bufferSize);

                try
                {
                    m_BufferManager.InitBuffer();
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error("Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                    return false;
                }

                // preallocate pool of SocketAsyncEventArgs objects
                SocketAsyncEventArgs socketEventArg;

                var socketArgsProxyList = new List<SocketAsyncEventArgsProxy>(AppServer.Config.MaxConnectionNumber);

                for (int i = 0; i < AppServer.Config.MaxConnectionNumber; i++)
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    socketEventArg = new SocketAsyncEventArgs();
                    m_BufferManager.SetBuffer(socketEventArg);

                    socketArgsProxyList.Add(new SocketAsyncEventArgsProxy(socketEventArg));
                }

                m_ReadWritePool = new ConcurrentStack<SocketAsyncEventArgsProxy>(socketArgsProxyList);

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

        protected override void OnNewClientAccepted(ISocketListener listener, Socket client, object state)
        {
            if (IsStopped)
                return;

            //Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            SocketAsyncEventArgsProxy socketEventArgsProxy;
            if (!m_ReadWritePool.TryPop(out socketEventArgsProxy))
            {
                AppServer.AsyncRun(client.SafeClose);
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.ErrorFormat("Max connection number {0} was reached!", AppServer.Config.MaxConnectionNumber);
                return;
            }

            ISocketSession session;

            var security = listener.Info.Security;

            if (security == SslProtocols.None)
                session = RegisterSession(client, new AsyncSocketSession(client, socketEventArgsProxy));
            else
                session = RegisterSession(client, new AsyncStreamSocketSession(client, security, socketEventArgsProxy));

            if (session == null)
            {
                socketEventArgsProxy.Reset();
                this.m_ReadWritePool.Push(socketEventArgsProxy);
                AppServer.AsyncRun(client.SafeClose);
                return;
            }

            session.Closed += SessionClosed;
            AppServer.AsyncRun(() => session.Start());
        }

        public override void ResetSessionSecurity(IAppSession session, SslProtocols security)
        {
            ISocketSession socketSession;

            var socketAsyncProxy = ((IAsyncSocketSessionBase)session.SocketSession).SocketAsyncProxy;

            if (security == SslProtocols.None)
                socketSession = new AsyncSocketSession(session.SocketSession.Client, socketAsyncProxy, true);
            else
                socketSession = new AsyncStreamSocketSession(session.SocketSession.Client, security, socketAsyncProxy, true);

            socketSession.Initialize(session);
            socketSession.Start();
        }

        void SessionClosed(ISocketSession session, CloseReason reason)
        {
            var socketSession = session as IAsyncSocketSessionBase;

            if (socketSession != null && this.m_ReadWritePool != null)
            {
                var proxy = socketSession.SocketAsyncProxy;
                proxy.Reset();

                if (proxy.OrigOffset != proxy.SocketEventArgs.Offset)
                {
                    proxy.SocketEventArgs.SetBuffer(proxy.OrigOffset, AppServer.Config.ReceiveBufferSize);
                }

                if (m_ReadWritePool != null)
                    m_ReadWritePool.Push(proxy);
            }
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

                m_ReadWritePool = null;
                m_BufferManager = null;
                IsRunning = false;
            }
        }
    }
}
