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
    class UdpSocketServer<TSocketSession, TAppSession> : SocketServerBase<TSocketSession, TAppSession>
        where TSocketSession : UdpSocketSession<TAppSession>, new()
        where TAppSession : IAppSession, new()
    {
        private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        private Semaphore m_MaxConnectionSemaphore;

        private ManualResetEvent m_UdpClientConnected = new ManualResetEvent(false);

        private BufferManager m_BufferManager;

        private SocketAsyncEventArgsPool m_ReadWritePool;

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

                m_ReadWritePool = new SocketAsyncEventArgsPool(2);

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

        void socketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                OnSocketReceived(e);
            }
        }

        private void OnSocketReceived(SocketAsyncEventArgs e)
        {
            string receivedNessage = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
            var address = e.RemoteEndPoint.Serialize();
            m_UdpClientConnected.Set();

            var ipAddress = EndPoint.Create(address) as IPEndPoint;
            TSocketSession session = RegisterSession(null);
            session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
            session.Start();
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

            m_MaxConnectionSemaphore = new Semaphore(AppServer.Config.MaxConnectionNumber, AppServer.Config.MaxConnectionNumber);

            IsRunning = true;

            OnStartupFinished();

            while (!IsStopped)
            {
                try
                {
                    m_UdpClientConnected.WaitOne();
                    m_MaxConnectionSemaphore.WaitOne();                    
                    SocketAsyncEventArgs eventArgs = m_ReadWritePool.Pop().SocketEventArgs;
                    m_ListenSocket.ReceiveFromAsync(eventArgs);
                    m_UdpClientConnected.Reset();
                }
                catch (ObjectDisposedException)
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
                    SocketException se = e as SocketException;
                    if (se != null)
                    {
                        //A blocking operation was interrupted by a call to WSACancelBlockingCall
                        //SocketListener has been stopped normally
                        if (se.ErrorCode == 10004 || se.ErrorCode == 10038)
                            return;
                    }

                    LogUtil.LogError(AppServer, "Socket Listener stopped unexpectly, Socket Address:" + EndPoint.Address.ToString() + ":" + EndPoint.Port, e);
                    return;
                }                
            }
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            m_MaxConnectionSemaphore.Release();
        }
    }
}
