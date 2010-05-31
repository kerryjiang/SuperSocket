using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;
using System.ServiceModel.Description;
using SuperSocket.Common;
using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Threading;

namespace SuperSocket.SocketServiceCore
{
    public interface ISocketServer
    {
        bool Start();
        void Stop();
    }

    public abstract class SocketServerBase<TSocketSession, TAppSession> : ISocketServer, IAsyncRunner
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>, new()        
	{
		protected object SyncRoot = new object();

		public IPEndPoint EndPoint { get; private set; }

        public IAppServer<TAppSession> AppServer { get; private set; }

        public SocketServerBase(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
        {
            AppServer = appServer;
            EndPoint = localEndPoint;
        }

        private Dictionary<string, TAppSession> m_SessionDict = new Dictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, TAppSession> SessionDict
        {
            get
            {
                return m_SessionDict;
            }
        }

        public int SessionCount
        {
            get { return m_SessionDict.Count; }
        }

        public virtual bool Start()
        {
            SetupClearSessionTimer();
            return true;
        }

        protected virtual TSocketSession RegisterSession(TcpClient client)
		{
            TSocketSession session = new TSocketSession();
            TAppSession appSession = this.AppServer.CreateAppSession(session);
            session.Initialize(this.AppServer, appSession, client);
			session.Closed += new EventHandler(session_Closed);

			lock (SyncRoot)
			{
                m_SessionDict[appSession.SessionID] = appSession;
			}

            LogUtil.LogInfo("SocketSession " + appSession.SessionID + " was accepted!");
			return session;
		}

		/// <summary>
		/// Handles the Closed event of the session control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void session_Closed(object sender, EventArgs e)
		{
			//the sender is a sessionID
			string sessionID = sender as string;

			if (!string.IsNullOrEmpty(sessionID))
			{
				try
				{
					SessionDict.Remove(sessionID);
					LogUtil.LogInfo("SocketSession " + sessionID + " was closed!");
				}
				catch (Exception exc)
				{
					LogUtil.LogError(exc);
				}
			}
		}

		public virtual void Stop()
		{
			if (m_ClearIdleSessionTimer != null)
			{
				m_ClearIdleSessionTimer.Enabled = false;
				m_ClearIdleSessionTimer.Dispose();
				m_ClearIdleSessionTimer = null;
			}

            SessionDict.Values.ToList().ForEach(s => s.Close());
		}

		protected void SetupClearSessionTimer()
		{
			m_ClearIdleSessionTimer = new System.Timers.Timer();
			m_ClearIdleSessionTimer.Interval = 60 * 1000;
			m_ClearIdleSessionTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_ClearIdleSessionTimer_Elapsed);
			m_ClearIdleSessionTimer.Enabled = true;
		}

		void m_ClearIdleSessionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			ClearIdleSession();
		}

		private System.Timers.Timer m_ClearIdleSessionTimer = null;

		private void ClearIdleSession()
		{
            lock (SyncRoot)
            {
                SessionDict.Values.Where(s =>
                    DateTime.Now.Subtract(s.SocketSession.LastActiveTime).TotalMinutes > 5)
                    .ToList().ForEach(s => s.Close());
            }
		}
        
	}
}
