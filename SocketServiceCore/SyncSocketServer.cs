using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Collections;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;
using System.IO;
using System.ServiceModel.Description;


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

        public SyncSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
            : base(appServer, localEndPoint)
		{
			
		}

		private TcpListener m_Listener = null;

		private Thread m_ListenThread = null;

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
					m_ListenThread.IsBackground = true;
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

			if (m_ListenThread != null)
			{
				m_ListenThread.Abort();
				m_ListenThread = null;
			}

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

			while (true)
			{
				TcpClient client = null;

				try
				{
					client = m_Listener.AcceptTcpClient();
				}
				catch (ThreadAbortException)
				{
					//Do nothing
				}
				catch (Exception e)
				{
					LogUtil.LogError("Socket Listener stopped unexpectly, Socket Address:" + EndPoint.Address.ToString() + ":" + EndPoint.Port);
					LogUtil.LogError(e);
				}

                TSocketSession session = RegisterSession(client);
								
				Thread thUser	= new Thread(session.Start);
				thUser.IsBackground = true;
				thUser.Start();				
			}
		}
		
	}
}
