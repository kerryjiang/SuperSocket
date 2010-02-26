using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Collections;
using GiantSoft.Common;
using GiantSoft.SocketServiceCore.Command;
using GiantSoft.SocketServiceCore.Config;
using System.IO;
using System.ServiceModel.Description;


namespace GiantSoft.SocketServiceCore
{
	/// <summary>
	/// The core socket server which can run any SocketSession
	/// </summary>
	/// <typeparam name="T">The typeof the SocketSession</typeparam>
	public abstract class SocketServer<T> : SocketServerBase<T>
		where T : SocketSession, new()
	{	

		public SocketServer()
			: base()
		{
			
		}

		public SocketServer(IPEndPoint localEndPoint)
            : base(localEndPoint)
		{
			
		}

		private TcpListener m_Listener = null;

		private Thread m_ListenThread = null;

		/// <summary>
		/// Starts the server
		/// </summary>
		public override bool Start()
		{
			if (!base.Start())
				return false;

			try
			{	
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
			
			IEnumerator enu = SessionDict.Values.GetEnumerator();
				
			while(enu.MoveNext())
			{
				SocketSession session = enu.Current as SocketSession;
				session.Close();	
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

				T session = RegisterSession(client);
								
				Thread thUser	= new Thread(session.Start);
				thUser.IsBackground = true;
				thUser.Start();				
			}
		}
		
	}
}
