using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;


namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// The core socket server which can run any SocketSession
    /// </summary>
    /// <typeparam name="T">The typeof the SocketSession</typeparam>
    class SyncSocketServer<TAppSession, TCommandInfo> : TcpSocketServerBase<SyncSocketSession<TAppSession, TCommandInfo>, TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public SyncSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, ICustomProtocol<TCommandInfo> protocol)
            : base(appServer, localEndPoint, protocol)
        {

        }

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
                AppServer.Logger.LogError(e);
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

                    AppServer.Logger.LogError("Socket Listener stopped unexpectly, Socket Address:" + EndPoint.Address.ToString() + ":" + EndPoint.Port, e);
                    break;
                }

                SyncSocketSession<TAppSession, TCommandInfo> session;

                try
                {
                    session = RegisterSession(client, new SyncSocketSession<TAppSession, TCommandInfo>(client, Protocol.CreateCommandReader(AppServer)));
                }
                catch (Exception e)
                {
                    AppServer.Logger.LogError(e);
                    continue;
                }

                if (session == null)
                {
                    Async.Run(() => client.SafeCloseClientSocket(AppServer.Logger));
                    continue;
                }

                session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
                
                //Start run session asynchronous
                Async.Run(() => session.Start(), TaskCreationOptions.LongRunning, (x) => AppServer.Logger.LogError(session, x));
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
