using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;


namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// The core socket server which can run any SocketSession
    /// </summary>
    /// <typeparam name="T">The typeof the SocketSession</typeparam>
    class SyncSocketServer<TAppSession, TCommandInfo> : TcpSocketServerBase<SyncSocketSession<TAppSession, TCommandInfo>, TAppSession>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public SyncSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, ISyncProtocol<TCommandInfo> protocol)
            : base(appServer, localEndPoint)
        {
            if (protocol == null)
                throw new ArgumentNullException("protocol");

            m_Protocol = protocol;
        }

        private ISyncProtocol<TCommandInfo> m_Protocol;

        private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        private Semaphore m_MaxConnectionSemaphore;

        /// <summary>
        /// Starts the server
        /// </summary>
        public override bool Start()
        {
            try
            {
                if (!base.Start())
                    return false;

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

        /// <summary>
        /// Starts to listen
        /// </summary>
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

            m_MaxConnectionSemaphore = new Semaphore(AppServer.Config.MaxConnectionNumber, AppServer.Config.MaxConnectionNumber);

            IsRunning = true;

            OnStartupFinished();

            while (!IsStopped)
            {
                Socket client = null;

                try
                {
                    m_MaxConnectionSemaphore.WaitOne();

                    if (IsStopped)
                        break;

                    client = m_ListenSocket.Accept();
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

                var session = RegisterSession(client, new SyncSocketSession<TAppSession, TCommandInfo>(m_Protocol.CreateSyncCommandReader()));
                session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);

                Thread thUser = new Thread(session.Start);
                thUser.IsBackground = true;
                thUser.Start();
            }

            IsRunning = false;
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            m_MaxConnectionSemaphore.Release();
        }

        /// <summary>
        /// Stops this server.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (IsRunning)
            {
                if (m_ListenSocket != null)
                {
                    m_ListenSocket.Close();
                    m_ListenSocket = null;
                }

                VerifySocketServerRunning(false);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                    Stop();

                m_MaxConnectionSemaphore.Close();
            }

            base.Dispose(disposing);
        }
    }
}
