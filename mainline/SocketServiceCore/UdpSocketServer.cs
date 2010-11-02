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

        private ManualResetEvent m_TcpClientConnected = new ManualResetEvent(false);

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
                Socket client = null;

                try
                {
                    m_MaxConnectionSemaphore.WaitOne();
                    //client = m_ListenSocket.ReceiveFromAsync()
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

                TSocketSession session = RegisterSession(client);
                //session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);

                Thread thUser = new Thread(session.Start);
                thUser.IsBackground = true;
                thUser.Start();
            }
        }
    }
}
