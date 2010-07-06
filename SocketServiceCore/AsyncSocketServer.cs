using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore
{
    public class AsyncSocketServer<TSocketSession, TAppSession> : SocketServerBase<TSocketSession, TAppSession>
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>, new()     
	{
        public AsyncSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
            : base(appServer, localEndPoint)
		{
			
		}

		private ManualResetEvent m_TcpClientConnected = new ManualResetEvent(false);

        private Semaphore m_MaxConnectionSemaphore;

		private TcpListener m_Listener = null;

        private Thread m_ListenThread = null;

        private bool m_Stopped = false;

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
			catch (Exception e)
			{
				LogUtil.LogError(e);
				return false;
			}			
		}

        private void StartListen()
        {
            m_Listener = new TcpListener(EndPoint);
            m_Listener.Start();
            m_Listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            m_MaxConnectionSemaphore = new Semaphore(this.AppServer.Config.MaxConnectionNumber, this.AppServer.Config.MaxConnectionNumber);

            while (!m_Stopped)
            {
                m_TcpClientConnected.Reset();
                m_Listener.BeginAcceptTcpClient(OnClientConnect, m_Listener);
                m_TcpClientConnected.WaitOne();
                m_MaxConnectionSemaphore.WaitOne();//two wait one here?
            }
        }

		public void OnClientConnect(IAsyncResult result)
		{
            TcpClient client = null;

            try
            {
                TcpListener listener = result.AsyncState as TcpListener;

                if (listener == null)
                    return;

                client = listener.EndAcceptTcpClient(result);
            }
            catch (ObjectDisposedException)//listener has been stopped
            {
                m_TcpClientConnected.Set();
                return;
            }
            catch (Exception e)
            {
                LogUtil.LogError("Failed to accept new tcp client in async server!", e);
                m_TcpClientConnected.Set();
                return;
            }

            TSocketSession session = RegisterSession(client);
            session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
            m_TcpClientConnected.Set();
			session.Start();
		}

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            m_MaxConnectionSemaphore.Release();
        }

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
	}
}
