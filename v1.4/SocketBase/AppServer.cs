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
    public abstract class AppServer : AppServer<AppSession>
    {
        public AppServer()
            : base()
        {

        }

        public AppServer(ICustomProtocol<StringCommandInfo> protocol)
            : base(protocol)
        {

        }
    }

    public abstract class AppServer<TAppSession> : AppServer<TAppSession, StringCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringCommandInfo>, new()
    {
        public AppServer()
            : base(new CommandLineProtocol())
        {

        }

        public AppServer(ICustomProtocol<StringCommandInfo> protocol)
            : base(protocol)
        {

        }
    }

    public abstract class AppServer<TAppSession, TCommandInfo> : AppServerBase<TAppSession, TCommandInfo>, IPerformanceDataSource
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TAppSession, TCommandInfo>, new()
    {

        private bool m_DisableSessionSnapshot;

        private int m_IdleSessionTimeOut;

        public AppServer()
            : base()
        {
            
        }

        protected AppServer(ICustomProtocol<TCommandInfo> protocol)
            : base(protocol)
        {
   
        }

        public override bool Start()
        {
            if (!base.Start())
                return false;

            if (!m_DisableSessionSnapshot)
                StartSessionSnapshotTimer();

            if (Config.ClearIdleSession)
                StartClearSessionTimer();

            return true;
        }

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<TCommandInfo> protocol)
        {
            m_DisableSessionSnapshot = config.DisableSessionSnapshot;
            m_IdleSessionTimeOut = config.IdleSessionTimeOut;
            return base.Setup(rootConfig, config, socketServerFactory, protocol);
        }

        private ConcurrentDictionary<string, TAppSession> m_SessionDict = new ConcurrentDictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);
                       
        public override TAppSession CreateAppSession(ISocketSession socketSession)
        {
            var appSession = base.CreateAppSession(socketSession);

            if (ReferenceEquals(NullAppSession, appSession))
                return appSession;

            if (m_SessionDict.TryAdd(appSession.IdentityKey, appSession))
            {
                Logger.LogInfo(appSession, "New SocketSession was accepted!");
                return appSession;
            }
            else
            {
                Logger.LogError(appSession, "SocketSession was refused because the session's IdentityKey already exists!");
                return NullAppSession;
            }
        }

        public override TAppSession GetAppSessionByIndentityKey(string identityKey)
        {
            if(string.IsNullOrEmpty(identityKey))
                return NullAppSession;

            TAppSession targetSession;
            m_SessionDict.TryGetValue(identityKey, out targetSession);
            return targetSession;
        }

        internal protected override void OnSocketSessionClosed(object sender, SocketSessionClosedEventArgs e)
        {
            //the sender is a sessionID
            string identityKey = e.IdentityKey;

            if (string.IsNullOrEmpty(identityKey))
                return;

            TAppSession removedSession;
            if (m_SessionDict.TryRemove(identityKey, out removedSession))
            {
                removedSession.Status = SessionStatus.Disconnected;
                Logger.LogInfo(removedSession, "This session was closed!");
                Async.Run(() => OnAppSessionClosed(this, new AppSessionClosedEventArgs<TAppSession>(removedSession, e.Reason)),
                    exc => Logger.LogError(exc));
            }
            else
            {
                Logger.LogError(removedSession, "Failed to remove this session, Because it haven't been in session container!");
            }
        }

        protected virtual void OnAppSessionClosed(object sender, AppSessionClosedEventArgs<TAppSession> e)
        {

        }

        public override int SessionCount
        {
            get
            {
                return m_SessionDict.Count;
            }
        }

        private KeyValuePair<string, TAppSession>[] SessionSource
        {
            get
            {
                if (m_DisableSessionSnapshot)
                    return m_SessionDict.ToArray();
                else
                    return m_SessionsSnapshot;
            }
        }

        #region Clear idle sessions

        private System.Threading.Timer m_ClearIdleSessionTimer = null;

        private void StartClearSessionTimer()
        {
            int interval = Config.ClearIdleSessionInterval * 1000;//in milliseconds
            m_ClearIdleSessionTimer = new System.Threading.Timer(ClearIdleSession, new object(), interval, interval);
        }

        private void ClearIdleSession(object state)
        {
            if (Monitor.TryEnter(state))
            {
                try
                {
                    DateTime now = DateTime.Now;
                    DateTime timeOut = now.AddSeconds(0 - m_IdleSessionTimeOut);

                    var timeOutSessions = SessionSource.Where(s => s.Value.LastActiveTime <= timeOut).Select(s => s.Value);
                    System.Threading.Tasks.Parallel.ForEach(timeOutSessions, s =>
                        {
                            Logger.LogInfo(s, string.Format("The socket session has been closed for {0} timeout, last active time: {1}!", now.Subtract(s.LastActiveTime).TotalSeconds, s.LastActiveTime));
                            s.Close(CloseReason.TimeOut);
                        });
                }
                catch (Exception e)
                {
                    Logger.LogError("Clear idle session error!", e);
                }
                finally
                {
                    Monitor.Exit(state);
                }
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
            return SessionSource.Select(p => p.Value).Where(critera);
        }

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<TAppSession> GetAllSessions()
        {
            return SessionSource.Select(p => p.Value);
        }

        #endregion

        #region Performance logging

        private PerformanceData m_PerformanceData = new PerformanceData();

        public PerformanceData CollectPerformanceData(GlobalPerformanceData globalPerfData)
        {
            m_PerformanceData.PushRecord(new PerformanceRecord
                {
                    TotalConnections = m_SessionDict.Count,
                    TotalHandledCommands = TotalHandledCommands
                });

            //User can process the performance data by self
            Async.Run(() => OnPerformanceDataCollected(globalPerfData, m_PerformanceData), e => Logger.LogError(e));

            return m_PerformanceData;
        }

        //User can override this method a get collected performance data
        protected virtual void OnPerformanceDataCollected(GlobalPerformanceData globalPerfData, PerformanceData performanceData)
        {

        }

        #endregion

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {                
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

                var sessions = m_SessionDict.ToArray();

                if(sessions.Length > 0)
                {
                    var tasks = new Task[sessions.Length];
                    
                    for(var i = 0; i < tasks.Length; i++)
                    {
                        tasks[i] = Task.Factory.StartNew((s) =>
                            {
                                ((TAppSession)s).Close(CloseReason.ServerShutdown);
                            }, sessions[i].Value);
                    }

                    Task.WaitAll(tasks);
                }
            }
        }

        #endregion
    }
}
