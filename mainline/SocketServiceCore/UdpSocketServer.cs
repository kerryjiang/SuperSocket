using System;
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
    class UdpSocketServer<TAppSession> : SocketServerBase<UdpSocketSession<TAppSession>, TAppSession>
        where TAppSession : IAppSession, new()
    {
        private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        private AutoResetEvent m_UdpClientConnected = new AutoResetEvent(false);

        private BufferManager m_BufferManager;

        private SynchronizedPool<SocketAsyncEventArgs> m_ReadWritePool;

        private int m_LiveConnectionCount = 0;

        public UdpSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
            : base(appServer, localEndPoint)
        {

        }

        public override bool Start()
        {
            try
            {
                if (!base.Start())
                    return false;

                int bufferSize = Math.Max(AppServer.Config.ReceiveBufferSize, AppServer.Config.SendBufferSize);

                if (bufferSize <= 0)
                    bufferSize = 1024 * 8;

                //m_BufferManager = new BufferManager(bufferSize * AppServer.Config.MaxConnectionNumber * 2, bufferSize);
                m_BufferManager = new BufferManager(bufferSize * 2, bufferSize);

                try
                {
                    m_BufferManager.InitBuffer();
                }
                catch (Exception e)
                {
                    LogUtil.LogError(AppServer, "Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                    return false;
                }

                m_ReadWritePool = new SynchronizedPool<SocketAsyncEventArgs>(2);

                // preallocate pool of SocketAsyncEventArgs objects
                SocketAsyncEventArgs socketEventArg;

                for (int i = 0; i < 1; i++)
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    socketEventArg = new SocketAsyncEventArgs();
                    socketEventArg.UserToken = new AsyncUserToken();
                    socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(socketEventArg_Completed);
                    m_BufferManager.SetBuffer(socketEventArg);
                    // add SocketAsyncEventArg to the pool
                    m_ReadWritePool.Push(socketEventArg);
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

        void socketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                OnSocketReceived(e);
            }
        }

        protected UdpSocketSession<TAppSession> RegisterSession(IPEndPoint remoteEndPoint)
        {
            UdpSocketSession<TAppSession> session = new UdpSocketSession<TAppSession>(m_ListenSocket, remoteEndPoint);
            TAppSession appSession = this.AppServer.CreateAppSession(session);
            session.Initialize(this.AppServer, appSession, null);
            return session;
        }

        private void OnSocketReceived(SocketAsyncEventArgs e)
        {
            string receivedNessage = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
            LogUtil.LogInfo("Server received: [" + receivedNessage + "]");
            if (receivedNessage.EndsWith(Environment.NewLine))
                receivedNessage = receivedNessage.Substring(0, receivedNessage.Length - Environment.NewLine.Length);

            var address = e.RemoteEndPoint.Serialize();

            m_ReadWritePool.Push(e);
            m_UdpClientConnected.Set();
     
            var ipAddress = EndPoint.Create(address) as IPEndPoint;
            TAppSession appSession = AppServer.GetAppSessionByIndentityKey(ipAddress.ToString());

            if (appSession == null) //New session
            {
                if (m_LiveConnectionCount >= AppServer.Config.MaxConnectionNumber)
                {
                    LogUtil.LogError(AppServer, string.Format("Cannot accept a new UDP connection from {0}, the max connection number {1} has been exceed!",
                        ipAddress.ToString(), AppServer.Config.MaxConnectionNumber));
                    return;
                }
                var session = RegisterSession(ipAddress);
                Interlocked.Increment(ref m_LiveConnectionCount);
                session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
                session.Start();
                session.ExecuteCommand(receivedNessage);
            }
            else //Existing session
            {
                var session = appSession.SocketSession as UdpSocketSession<TAppSession>;
                session.ExecuteCommand(receivedNessage);
            }
        }

        private void StartListen()
        {
            m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                m_ListenSocket.Bind(this.EndPoint);
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
                OnStartupFinished();
                return;
            }

            IsRunning = true;

            OnStartupFinished();

            while (!IsStopped)
            {
                try
                {
                    SocketAsyncEventArgs eventArgs = m_ReadWritePool.Pop();
                    eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    m_ListenSocket.ReceiveFromAsync(eventArgs);
                    m_UdpClientConnected.WaitOne();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (Exception e)
                {
                    SocketException se = e as SocketException;
                    if (se != null)
                    {
                        //A blocking operation was interrupted by a call to WSACancelBlockingCall
                        //SocketListener has been stopped normally
                        if (se.ErrorCode == 10004 || se.ErrorCode == 10038)
                            break;
                    }

                    LogUtil.LogError(AppServer, "Socket Listener stopped unexpectly, Socket Address:" + EndPoint.Address.ToString() + ":" + EndPoint.Port, e);
                    break;
                }                
            }

            IsRunning = false;
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            Interlocked.Increment(ref m_LiveConnectionCount);
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

                m_UdpClientConnected.Close();
            }

            base.Dispose(disposing);
        }
    }
}
