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
    class AsyncSocketServer<TAppSession, TCommandInfo> : TcpSocketServerBase<AsyncSocketSession<TAppSession, TCommandInfo>, TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public AsyncSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, ICustomProtocol<TCommandInfo> protocol)
            : base(appServer, localEndPoint, protocol)
        {

        }

        private AutoResetEvent m_TcpClientConnected;

        private BufferManager m_BufferManager;

        private ConcurrentStack<SocketAsyncEventArgsProxy> m_ReadWritePool;

        private Semaphore m_MaxConnectionSemaphore;

        private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        public override bool Start()
        {
            try
            {
                if (!base.Start())
                    return false;

                m_TcpClientConnected = new AutoResetEvent(false);

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
                    AppServer.Logger.LogError("Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
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

                if (m_ListenSocket == null)
                {
                    m_ListenThread = new Thread(StartListen);
                    m_ListenThread.Start();
                }

                WaitForStartupFinished();

                return IsRunning;
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(e);
                return false;
            }
        }

        private void StartListen()
        {
            m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_ListenSocket.Bind(this.EndPoint);
                m_ListenSocket.Listen(this.AppServer.Config.ListenBacklog);

                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(e);
                OnStartupFinished();
                return;
            }

            m_MaxConnectionSemaphore = new Semaphore(this.AppServer.Config.MaxConnectionNumber, this.AppServer.Config.MaxConnectionNumber);

            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArg_Completed);

            IsRunning = true;

            OnStartupFinished();

            while (!IsStopped)
            {
                m_MaxConnectionSemaphore.WaitOne();

                if (IsStopped)
                    break;

                acceptEventArg.AcceptSocket = null;

                bool willRaiseEvent = true;

                try
                {
                    willRaiseEvent = m_ListenSocket.AcceptAsync(acceptEventArg);
                }
                catch (ObjectDisposedException)//listener has been stopped
                {
                    break;
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (Exception e)
                {
                    AppServer.Logger.LogError("Failed to accept new tcp client in async server!", e);
                    break;
                }

                if (!willRaiseEvent)
                    AceptNewClient(acceptEventArg);

                m_TcpClientConnected.WaitOne();
            }

            IsRunning = false;
        }

        void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            AceptNewClient(e);
        }

        void AceptNewClient(SocketAsyncEventArgs e)
        {            
            if (e.SocketError == SocketError.Success)
            {
                var client = e.AcceptSocket;
                m_TcpClientConnected.Set();

                //Get the socket for the accepted client connection and put it into the 
                //ReadEventArg object user token
                SocketAsyncEventArgsProxy socketEventArgsProxy;
                if (!m_ReadWritePool.TryPop(out socketEventArgsProxy))
                {
                    AppServer.Logger.LogError("There is no enough buffer block to arrange to new accepted client!");
                    return;
                }

                var session = RegisterSession(client, new AsyncSocketSession<TAppSession, TCommandInfo>(client, Protocol.CreateCommandReader(AppServer)));
                
                if (session != null)
                {
                    session.SocketAsyncProxy = socketEventArgsProxy;
                    session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
                    session.Start();
                }
                else
                {
                    Async.Run(() => client.SafeCloseClientSocket(AppServer.Logger));
                }
            }
            else
            {
                m_TcpClientConnected.Set();
            }
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            m_MaxConnectionSemaphore.Release();

            IAsyncSocketSession socketSession = sender as IAsyncSocketSession;
            if (socketSession != null && this.m_ReadWritePool != null)
                this.m_ReadWritePool.Push(socketSession.SocketAsyncProxy);
        }

        public override void Stop()
        {
            base.Stop();

            if (m_ListenSocket != null)
            {
                m_ListenSocket.Close();
                m_ListenSocket = null;
            }

            if (m_ReadWritePool != null)
                m_ReadWritePool = null;

            if (m_BufferManager != null)
                m_BufferManager = null;

            VerifySocketServerRunning(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                    Stop();

                m_TcpClientConnected.Close();
                m_MaxConnectionSemaphore.Close();
            }

            base.Dispose(disposing);
        }
    }
}
