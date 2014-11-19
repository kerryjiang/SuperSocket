using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Security;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// AppServer class
    /// </summary>
    public class AppServer : AppServer<AppSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer"/> class.
        /// </summary>
        public AppServer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServer(IReceiveFilterFactory<StringRequestInfo> receiveFilterFactory)
            : base(receiveFilterFactory)
        {

        }
    }

    /// <summary>
    /// AppServer class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public class AppServer<TAppSession> : AppServer<TAppSession, StringRequestInfo>
        where TAppSession : AppSession<TAppSession, StringRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession&gt;"/> class.
        /// </summary>
        public AppServer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession&gt;"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServer(IReceiveFilterFactory<StringRequestInfo> receiveFilterFactory)
            : base(receiveFilterFactory)
        {

        }

        internal override IReceiveFilterFactory<StringRequestInfo> CreateDefaultReceiveFilterFactory()
        {
            return new CommandLineReceiveFilterFactory(TextEncoding);
        }
    }


    /// <summary>
    /// AppServer basic class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class AppServer<TAppSession, TRequestInfo> : AppServerBase<TAppSession, TRequestInfo>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        public AppServer()
            : base()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        protected AppServer(IReceiveFilterFactory<TRequestInfo> protocol)
            : base(protocol)
        {
   
        }

        internal override IReceiveFilterFactory<TRequestInfo> CreateDefaultReceiveFilterFactory()
        {
            return null;
        }

        /// <summary>
        /// Starts this AppServer instance.
        /// </summary>
        /// <returns></returns>
        public override bool Start()
        {
            if (!base.Start())
                return false;

            if (!Config.DisableSessionSnapshot)
                StartSessionSnapshotTimer();

            if (Config.ClearIdleSession)
                StartClearSessionTimer();

            return true;
        }

        private ConcurrentDictionary<string, TAppSession> m_SessionDict = new ConcurrentDictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers the session into the session container.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="appSession">The app session.</param>
        /// <returns></returns>
        protected override bool RegisterSession(string sessionID, TAppSession appSession)
        {
            if (m_SessionDict.TryAdd(sessionID, appSession))
                return true;

            if (Logger.IsErrorEnabled)
                Logger.Error(appSession, "The session is refused because the it's ID already exists!");

            return false;
        }

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        [Obsolete("Use the method GetSessionByID instead")]
        public TAppSession GetAppSessionByID(string sessionID)
        {
            return GetSessionByID(sessionID);
        }

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        public override TAppSession GetSessionByID(string sessionID)
        {
            if (string.IsNullOrEmpty(sessionID))
                return NullAppSession;

            TAppSession targetSession;
            m_SessionDict.TryGetValue(sessionID, out targetSession);
            return targetSession;
        }

        /// <summary>
        /// Called when [socket session closed].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="reason">The reason.</param>
        protected override void OnSessionClosed(TAppSession session, CloseReason reason)
        {
            string sessionID = session.SessionID;

            if (!string.IsNullOrEmpty(sessionID))
            {
                TAppSession removedSession;
                if (!m_SessionDict.TryRemove(sessionID, out removedSession))
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(session, "Failed to remove this session, Because it has't been in session container!");
                }
            }

            base.OnSessionClosed(session, reason);
        }

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        public override int SessionCount
        {
            get
            {
                return m_SessionDict.Count;
            }
        }

        #region Clear idle sessions

        private System.Threading.Timer m_ClearIdleSessionTimer = null;

        private void StartClearSessionTimer()
        {
            int interval = Config.ClearIdleSessionInterval * 1000;//in milliseconds
            m_ClearIdleSessionTimer = new System.Threading.Timer(ClearIdleSession, new object(), interval, interval);
        }

        /// <summary>
        /// Clears the idle session.
        /// </summary>
        /// <param name="state">The state.</param>
        private void ClearIdleSession(object state)
        {
            if (Monitor.TryEnter(state))
            {
                try
                {
                    var sessionSource = SessionSource;

                    if (sessionSource == null)
                        return;

                    DateTime now = DateTime.Now;
                    DateTime timeOut = now.AddSeconds(0 - Config.IdleSessionTimeOut);

                    var timeOutSessions = sessionSource.Where(s => s.Value.LastActiveTime <= timeOut).Select(s => s.Value);

                    System.Threading.Tasks.Parallel.ForEach(timeOutSessions, s =>
                        {
                            if (Logger.IsInfoEnabled)
                                Logger.Info(s, string.Format("The session will be closed for {0} timeout, the session start time: {1}, last active time: {2}!", now.Subtract(s.LastActiveTime).TotalSeconds, s.StartTime, s.LastActiveTime));
                            s.Close(CloseReason.TimeOut);
                        });
                }
                catch (Exception e)
                {
                    if(Logger.IsErrorEnabled)
                        Logger.Error("Clear idle session error!", e);
                }
                finally
                {
                    Monitor.Exit(state);
                }
            }
        }

        private KeyValuePair<string, TAppSession>[] SessionSource
        {
            get
            {
                if (Config.DisableSessionSnapshot)
                    return m_SessionDict.ToArray();
                else
                    return m_SessionsSnapshot;
            }
        }

        #endregion

        #region Take session snapshot

        private System.Threading.Timer m_SessionSnapshotTimer = null;

        private KeyValuePair<string, TAppSession>[] m_SessionsSnapshot = new KeyValuePair<string, TAppSession>[0];

        private void StartSessionSnapshotTimer()
        {
            int interval = Math.Max(Config.SessionSnapshotInterval, 1) * 1000;//in milliseconds
            m_SessionSnapshotTimer = new System.Threading.Timer(TakeSessionSnapshot, new object(), interval, interval);
        }

        private void TakeSessionSnapshot(object state)
        {
            if (Monitor.TryEnter(state))
            {
                Interlocked.Exchange(ref m_SessionsSnapshot, m_SessionDict.ToArray());
                Monitor.Exit(state);
            }
        }

        #endregion

        #region Search session utils

        /// <summary>
        /// Gets the matched sessions from sessions snapshot.
        /// </summary>
        /// <param name="critera">The prediction critera.</param>
        /// <returns></returns>
        public override IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
        {
            var sessionSource = SessionSource;

            if (sessionSource == null)
                return null;

            return sessionSource.Select(p => p.Value).Where(critera);
        }

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<TAppSession> GetAllSessions()
        {
            var sessionSource = SessionSource;

            if (sessionSource == null)
                return null;

            return sessionSource.Select(p => p.Value);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (m_SessionSnapshotTimer != null)
            {
                m_SessionSnapshotTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_SessionSnapshotTimer.Dispose();
                m_SessionSnapshotTimer = null;
            }

            if (m_ClearIdleSessionTimer != null)
            {
                m_ClearIdleSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_ClearIdleSessionTimer.Dispose();
                m_ClearIdleSessionTimer = null;
            }

            m_SessionsSnapshot = null;

            var sessions = m_SessionDict.ToArray();

            if (sessions.Length > 0)
            {
                var tasks = new Task[sessions.Length];

                for (var i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Factory.StartNew((s) =>
                    {
                        var session = s as TAppSession;

                        if (session != null)
                        {
                            session.Close(CloseReason.ServerShutdown);
                        }

                    }, sessions[i].Value);
                }

                Task.WaitAll(tasks);
            }
        }

        #endregion
    }
}
