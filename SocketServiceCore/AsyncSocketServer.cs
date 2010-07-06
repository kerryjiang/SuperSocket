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

		private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        private bool m_Stopped = false;

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
				LogUtil.LogError(e);
				return false;
			}			
		}

        private void StartListen()
        {
            m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(this.EndPoint);
            m_ListenSocket.Listen(100);

            m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            m_MaxConnectionSemaphore = new Semaphore(this.AppServer.Config.MaxConnectionNumber, this.AppServer.Config.MaxConnectionNumber);

            while (!m_Stopped)
            {
                m_TcpClientConnected.Reset();
                m_ListenSocket.BeginAccept(OnClientConnect, m_ListenSocket);
                m_TcpClientConnected.WaitOne();
                m_MaxConnectionSemaphore.WaitOne();//two wait one here?
            }
        }

		public void OnClientConnect(IAsyncResult result)
		{
            Socket clientSocket = null;

            try
            {
                Socket listener = result.AsyncState as Socket;

                if (listener == null)
                    return;

                clientSocket = listener.EndAccept(result);
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

            TSocketSession session = RegisterSession(clientSocket);
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

            if (m_ListenSocket != null)
			{
                m_ListenSocket.Close();
                m_ListenSocket = null;
			}

            m_Stopped = true;
		}
	}
}
