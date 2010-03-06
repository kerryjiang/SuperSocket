using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;
using System.Threading;
using System.Collections;

namespace SuperSocket.SocketServiceCore
{
	public abstract class AsyncSocketServer<T> : SocketServerBase<T>
		where T : SocketSession, new()
	{
		public static ManualResetEvent m_TcpClientConnected = new ManualResetEvent(false);

		private TcpListener m_Listener = null;

		public override bool Start()
		{
			if (!base.Start())
				return false;

			try
			{
				m_Listener = new TcpListener(EndPoint);
				m_Listener.Start();
				m_Listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

				while (true)
				{
					m_TcpClientConnected.Reset();

					m_Listener.BeginAcceptTcpClient(OnClientConnect, null);

					m_TcpClientConnected.WaitOne();

					break;
				}

				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				return false;
			}			
		}

		public void OnClientConnect(IAsyncResult result)
		{
			TcpClient client = m_Listener.EndAcceptTcpClient(result);
			T session = RegisterSession(client);
			session.Start();
		}

		public override void Stop()
		{
			base.Stop();

			if (m_Listener != null)
			{
				m_Listener.Stop();
				m_Listener = null;
			}

			IEnumerator enu = SessionDict.Values.GetEnumerator();

			while (enu.MoveNext())
			{
				SocketSession session = enu.Current as SocketSession;
				session.Close();
			}
		}
	}
}
