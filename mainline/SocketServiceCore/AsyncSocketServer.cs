using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.AsyncSocket;

namespace SuperSocket.SocketServiceCore
{
    class AsyncSocketServer<TSocketSession, TAppSession> : SocketServerBase<TSocketSession, TAppSession>, IAsyncRunner
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>, IAsyncSocketSession, new()
    {
        public AsyncSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
            : base(appServer, localEndPoint)
        {

        }

        private ManualResetEvent m_TcpClientConnected = new ManualResetEvent(false);

        private BufferManager m_BufferManager;

        private SocketAsyncEventArgsPool m_ReadWritePool;

        private Semaphore m_MaxConnectionSemaphore;

        private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        public override bool Start()
        {
            try
            {
                if (!base.Start())
                    return false;

                int bufferSize = Math.Max(AppServer.Config.ReceiveBufferSize, AppServer.Config.SendBufferSize);

                if (bufferSize <= 0)
                    bufferSize = 1024 * 8;

                m_BufferManager = new BufferManager(bufferSize * AppServer.Config.MaxConnectionNumber * 2, bufferSize);

                try
                {
                    m_BufferManager.InitBuffer();
                }
                catch (Exception e)
                {
                    LogUtil.LogError(AppServer, "Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                    return false;
                }

                m_ReadWritePool = new SocketAsyncEventArgsPool(AppServer.Config.MaxConnectionNumber);

                // preallocate pool of SocketAsyncEventArgs objects
                SocketAsyncEventArgs socketEventArg;

                for (int i = 0; i < AppServer.Config.MaxConnectionNumber; i++)
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    socketEventArg = new SocketAsyncEventArgs();
                    socketEventArg.UserToken = new AsyncUserToken();
                    m_BufferManager.SetBuffer(socketEventArg);

                    // add SocketAsyncEventArg to the pool
                    m_ReadWritePool.Push(new SocketAsyncEventArgsProxy(socketEventArg));
                }                

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
                LogUtil.LogError(AppServer, e);
                return false;
            }
        }

        private void StartListen()
        {
            m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_ListenSocket.Bind(this.EndPoint);
                m_ListenSocket.Listen(100);

                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
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
                m_TcpClientConnected.Reset();
                acceptEventArg.AcceptSocket = null;

                bool willRaiseEvent = true;

                try
                {
                    willRaiseEvent = m_ListenSocket.AcceptAsync(acceptEventArg);
                }
                catch (ObjectDisposedException)//listener has been stopped
                {
                    IsRunning = false;
                    return;
                }
                catch (NullReferenceException)
                {
                    IsRunning = false;
                    return;
                }
                catch (Exception e)
                {
                    IsRunning = false;
                    LogUtil.LogError(AppServer, "Failed to accept new tcp client in async server!", e);
                    return;
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
                //Get the socket for the accepted client connection and put it into the 
                //ReadEventArg object user token
                SocketAsyncEventArgsProxy socketEventArgsProxy = m_ReadWritePool.Pop();
                socketEventArgsProxy.Socket = e.AcceptSocket;

                TSocketSession session = RegisterSession(e.AcceptSocket);
                session.SocketAsyncProxy = socketEventArgsProxy;
                session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);

                m_TcpClientConnected.Set();
                session.Start();
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
