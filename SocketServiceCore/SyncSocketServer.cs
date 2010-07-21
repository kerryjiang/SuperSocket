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
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;


namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// The core socket server which can run any SocketSession
    /// </summary>
    /// <typeparam name="T">The typeof the SocketSession</typeparam>
    public class SyncSocketServer<TSocketSession, TAppSession> : SocketServerBase<TSocketSession, TAppSession>
        where TSocketSession : ISocketSession<TAppSession>, new()
        where TAppSession : IAppSession, new()
    {

        private bool m_Stopped = false;

        public SyncSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
            : base(appServer, localEndPoint)
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

                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
                return false;
            }
        }

        /// <summary>
        /// Stops this server.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (m_ListenSocket != null)
            {
                m_ListenSocket.Close();
                m_ListenSocket = null;
            }

            m_Stopped = true;
        }

        /// <summary>
        /// Starts to listen
        /// </summary>
        private void StartListen()
        {
            m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(this.EndPoint);
            m_ListenSocket.Listen(100);

            m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            m_MaxConnectionSemaphore = new Semaphore(AppServer.Config.MaxConnectionNumber, AppServer.Config.MaxConnectionNumber);

            while (!m_Stopped)
            {
                Socket client = null;

                try
                {
                    m_MaxConnectionSemaphore.WaitOne();
                    client = m_ListenSocket.Accept();
                }
                catch (ObjectDisposedException)
                {
                    //Do nothing
                    return;
                }
                catch (Exception e)
                {
                    SocketException se = e as SocketException;
                    if (se != null)
                    {
                        //A blocking operation was interrupted by a call to WSACancelBlockingCall
                        //SocketListener has been stopped normally
                        if (se.ErrorCode == 10004)
                            return;
                    }

                    LogUtil.LogError(AppServer, "Socket Listener stopped unexpectly, Socket Address:" + EndPoint.Address.ToString() + ":" + EndPoint.Port);
                    LogUtil.LogError(AppServer, e);
                    return;
                }

                TSocketSession session = RegisterSession(client);
                session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);

                Thread thUser = new Thread(session.Start);
                thUser.IsBackground = true;
                thUser.Start();
            }
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            m_MaxConnectionSemaphore.Release();
        }

    }
}
