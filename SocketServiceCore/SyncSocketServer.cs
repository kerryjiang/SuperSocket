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

		private TcpListener m_Listener = null;

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

				if (m_Listener == null)
				{
					m_ListenThread = new Thread(StartListen);
                    m_ListenThread.Start();					
				}

                return true;
			}
			catch(Exception e)
			{
				LogUtil.LogError(e);
				return false;
			}
		}

		/// <summary>
		/// Stops this server.
		/// </summary>
		public override void Stop()
		{
			base.Stop();

            m_Stopped = true;

			if (m_Listener != null)
			{
				m_Listener.Stop();
				m_Listener = null;
			}		
		}

		/// <summary>
		/// Starts to listen
		/// </summary>
		private void StartListen()
		{
			m_Listener = new TcpListener(EndPoint);
			m_Listener.Start();
			m_Listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            m_MaxConnectionSemaphore = new Semaphore(AppServer.Config.MaxConnectionNumber, AppServer.Config.MaxConnectionNumber);

            while (!m_Stopped)
			{
				TcpClient client = null;

                try
                {
                    m_MaxConnectionSemaphore.WaitOne();
                    client = m_Listener.AcceptTcpClient();
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

                    LogUtil.LogError("Socket Listener stopped unexpectly, Socket Address:" + EndPoint.Address.ToString() + ":" + EndPoint.Port);
                    LogUtil.LogError(e);
                    return;
                }

                TSocketSession session = RegisterSession(client);
                session.Closed +=new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
								
				Thread thUser	= new Thread(session.Start);
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
