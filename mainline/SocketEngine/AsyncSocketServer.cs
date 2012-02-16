using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
                    socketEventArg.UserToken = new AsyncUserToken();
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

        protected override void AcceptNewClient(ISocketListener listener, Socket client)
        {
            if (IsStopped)
                return;

            //Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            SocketAsyncEventArgsProxy socketEventArgsProxy;
            if (!m_ReadWritePool.TryPop(out socketEventArgsProxy))
            {
                Async.Run(() => client.SafeCloseClientSocket(AppServer.Logger));
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.ErrorFormat("Max connection number {0} was reached!", AppServer.Config.MaxConnectionNumber);
                return;
            }

            var session = RegisterSession(client, new AsyncSocketSession(client));

            if (session == null)
            {
                this.m_ReadWritePool.Push(socketEventArgsProxy);
                Async.Run(() => client.SafeCloseClientSocket(AppServer.Logger));
            }

            ((IAsyncSocketSession)session).SocketAsyncProxy = socketEventArgsProxy;
            session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
            Async.Run(() => session.Start());
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            IAsyncSocketSession socketSession = sender as IAsyncSocketSession;
            if (socketSession != null && this.m_ReadWritePool != null)
                this.m_ReadWritePool.Push(socketSession.SocketAsyncProxy);
        }

        public override void Stop()
        {
            base.Stop();

            if (m_ReadWritePool != null)
                m_ReadWritePool = null;

            if (m_BufferManager != null)
                m_BufferManager = null;

            IsRunning = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                    Stop();
            }

            base.Dispose(disposing);
        }
    }
}
