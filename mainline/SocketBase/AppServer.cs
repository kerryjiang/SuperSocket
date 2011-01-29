using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Security;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase
{
    public abstract class AppServer<TAppSession> : AppServer<TAppSession, StringCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringCommandInfo>, new()
    {
        public AppServer()
            : base()
        {
            Protocol = new CommandLineProtocol();
        }
    }

    public abstract class AppServer<TAppSession, TCommandInfo> : IAppServer<TAppSession, TCommandInfo>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TAppSession, TCommandInfo>, new()
    {
        private IPEndPoint m_LocalEndPoint;

        public IServerConfig Config { get; private set; }

        protected virtual ConsoleHostInfo ConsoleHostInfo { get { return null; } }

        public virtual X509Certificate Certificate { get; protected set; }

        public virtual ICustomProtocol<TCommandInfo> Protocol { get; protected set; }

        private string m_ConsoleBaseAddress;

        public ServiceCredentials ServerCredentials { get; set; }

        private Dictionary<string, ICommand<TAppSession, TCommandInfo>> m_CommandDict = new Dictionary<string, ICommand<TAppSession, TCommandInfo>>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, ProviderBase<TAppSession>> m_ProviderDict = new Dictionary<string, ProviderBase<TAppSession>>(StringComparer.OrdinalIgnoreCase);

        private ISocketServerFactory m_SocketServerFactory;

        public SslProtocols BasicSecurity { get; private set; }

        protected IRootConfig RootConfig { get; private set; }

        public ILogger Logger { get; private set; }

        private static bool m_ThreadPoolConfigured = false;

        public AppServer()
        {
            
        } 

        protected virtual bool SetupCommands(Dictionary<string, ICommand<TAppSession, TCommandInfo>> commandDict)
        {
            var commandLoader = new ReflectCommandLoader<ICommand<TAppSession, TCommandInfo>>(typeof(TAppSession).Assembly);

            foreach (var command in commandLoader.LoadCommands())
            {
                Logger.LogDebug(string.Format("Command found: {0} - {1}", command.Name, command.GetType().AssemblyQualifiedName));
                //command.DefaultParameterParser = CommandParameterParser;
                if (commandDict.ContainsKey(command.Name))
                {
                    Logger.LogError("Duplicated name command has been found! Command name: " + command.Name);
                    return false;
                }

                commandDict.Add(command.Name, command);
            }

            return true;
        }

        private ServiceHost CreateConsoleHost(ConsoleHostInfo consoleInfo)
        {
            Binding binding = new BasicHttpBinding();

            var host = new ServiceHost(consoleInfo.ServiceInstance, new Uri(m_ConsoleBaseAddress + Name));

            foreach (var contract in consoleInfo.ServiceContracts)
            {
                host.AddServiceEndpoint(contract, binding, contract.Name);
            }

            return host;
        }

        private ServiceHost m_ConsoleHost;

        private bool StartConsoleHost()
        {
            var consoleInfo = ConsoleHostInfo;

            if (consoleInfo == null)
                return true;

            m_ConsoleHost = CreateConsoleHost(consoleInfo);

            try
            {
                m_ConsoleHost.Open();
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                m_ConsoleHost = null;
                return false;
            }
        }

        private void CloseConsoleHost()
        {
            if (m_ConsoleHost == null)
                return;

            try
            {
                m_ConsoleHost.Close();
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to close console service host!", e);
            }
            finally
            {
                m_ConsoleHost = null;
            }
        }

        public bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory)
        {
            return Setup(rootConfig, config, socketServerFactory, null, string.Empty);
        }

        public bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<TCommandInfo> protocol)
        {
            return Setup(rootConfig, config, socketServerFactory, protocol, string.Empty);
        }

        public bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, string providerAssembly)
        {
            return Setup(rootConfig, config, socketServerFactory, null, providerAssembly);
        }

        public virtual bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<TCommandInfo> protocol, string assembly)
        {
            if (rootConfig == null)
                throw new ArgumentNullException("rootConfig");

            RootConfig = rootConfig;

            if (!m_ThreadPoolConfigured)
            {
                if (!TheadPoolEx.ResetThreadPool(rootConfig.MaxWorkingThreads >= 0 ? rootConfig.MaxWorkingThreads : new Nullable<int>(),
                        rootConfig.MaxCompletionPortThreads >= 0 ? rootConfig.MaxCompletionPortThreads : new Nullable<int>(),
                        rootConfig.MinWorkingThreads >= 0 ? rootConfig.MinWorkingThreads : new Nullable<int>(),
                        rootConfig.MinCompletionPortThreads >= 0 ? rootConfig.MinCompletionPortThreads : new Nullable<int>()))
                {
                    return false;
                }

                m_ThreadPoolConfigured = true;
            }

            if (config == null)
                throw new ArgumentNullException("config");

            Config = config;

            m_ConsoleBaseAddress = rootConfig.ConsoleBaseAddress;
            m_SocketServerFactory = socketServerFactory;

            SetupLogger();

            if (!SetupLocalEndpoint(config))
            {
                Logger.LogError("Invalid config ip/port");
                return false;
            }

            if (!SetupProtocol(config, protocol))
                return false;

            if (!SetupCommands(m_CommandDict))
                return false;

            if (!string.IsNullOrEmpty(assembly))
            {
                if (!SetupServiceProviders(config, assembly))
                    return false;
            }

            if (!SetupSecurity(config))
                return false;

            return SetupSocketServer();
        }

        private bool SetupProtocol(IServerConfig config, ICustomProtocol<TCommandInfo> protocol)
        {
            //The protocol passed by programming has higher priority, then by config
            if (protocol != null)
            {
                Protocol = protocol;
            }
            else
            {
                //There is a protocol configuration existing
                if (!string.IsNullOrEmpty(config.Protocol))
                {
                    ICustomProtocol<TCommandInfo> configuredProtocol;
                    if (!AssemblyUtil.TryCreateInstance<ICustomProtocol<TCommandInfo>>(config.Protocol, out configuredProtocol))
                    {
                        Logger.LogError("Invalid configured protocol " + config.Protocol + ".");
                        return false;
                    }
                    Protocol = configuredProtocol;
                }
            }

            //If there is no defined protocol, use CommandLineProtocol as default
            if (Protocol == null)
            {
                Logger.LogError("Protocol hasn't been set!");
                return false;
            }

            return true;
        }

        private void SetupLogger()
        {
            switch (RootConfig.LoggingMode)
            {
                case (LoggingMode.IndependantFile):
                    Logger = new DynamicLog4NetLogger(Config.Name);
                    break;

                case (LoggingMode.Console):
                    Logger = new ConsoleLogger(Config.Name);
                    break;

                default:
                    Logger = new Log4NetLogger(Config.Name);
                    break;
            }
        }

        private bool SetupSecurity(IServerConfig config)
        {
            if (!string.IsNullOrEmpty(config.Security))
            {
                SslProtocols configProtocol;
                if (!config.Security.TryParseEnum<SslProtocols>(true, out configProtocol))
                {
                    Logger.LogError(string.Format("Failed to parse '{0}' to SslProtocol!", config.Security));
                    return false;
                }

                if (configProtocol != SslProtocols.None)
                {
                    //SSL/TLS is only supported in Sync mode
                    if (config.Mode != SocketMode.Sync)
                    {
                        Logger.LogError("You cannot enable SSL/TLS security for your current SocketMode, it only can be used in Sync mode!");
                        return false;
                    }

                    if (config.Certificate == null || !config.Certificate.IsEnabled)
                    {
                        Logger.LogError("There is no certificate defined and enabled!");
                        return false;
                    }

                    if (!SetupCertificate(config))
                        return false;
                }

                BasicSecurity = configProtocol;
            }
            else
            {
                BasicSecurity = SslProtocols.None;
            }

            return true;
        }

        private bool SetupSocketServer()
        {
            try
            {
                m_SocketServer = m_SocketServerFactory.CreateSocketServer<TAppSession, TCommandInfo>(this, m_LocalEndPoint, Config, Protocol);
                return m_SocketServer != null;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return false;
            }
        }

        private bool SetupLocalEndpoint(IServerConfig config)
        {
            if (config.Port > 0)
            {
                try
                {
                    if (string.IsNullOrEmpty(config.Ip) || "Any".Equals(config.Ip, StringComparison.OrdinalIgnoreCase))
                        m_LocalEndPoint = new IPEndPoint(IPAddress.Any, config.Port);
                    else if ("IPv6Any".Equals(config.Ip, StringComparison.OrdinalIgnoreCase))
                        m_LocalEndPoint = new IPEndPoint(IPAddress.IPv6Any, config.Port);
                    else
                        m_LocalEndPoint = new IPEndPoint(IPAddress.Parse(config.Ip), config.Port);

                    return true;
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    return false;
                }
            }

            return false;
        }

        private bool SetupCertificate(IServerConfig config)
        {
            try
            {
                Certificate = CertificateManager.Initialize(config.Certificate);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to initialize certificate!", e);
                return false;
            }
        }

        protected ProviderBase<TAppSession> GetProviderByName(string providerName)
        {
            ProviderBase<TAppSession> provider = null;

            if (m_ProviderDict.TryGetValue(providerName, out provider))
            {
                return provider;
            }
            else
            {
                return null;
            }
        }

        private bool SetupServiceProviders(IServerConfig config, string assembly)
        {
            string dir = Path.GetDirectoryName(this.GetType().Assembly.Location);

            string assemblyFile = Path.Combine(dir, assembly + ".dll");

            try
            {
                Assembly ass = Assembly.LoadFrom(assemblyFile);

                foreach (var providerType in ass.GetImplementTypes<ProviderBase<TAppSession>>())
                {
                    ProviderBase<TAppSession> provider = ass.CreateInstance(providerType.ToString()) as ProviderBase<TAppSession>;

                    if (provider.Init(this, config))
                    {
                        m_ProviderDict[provider.Name] = provider;
                    }
                    else
                    {
                        Logger.LogError("Failed to initalize provider " + providerType.ToString() + "!");
                        return false;
                    }
                }

                if (!IsReady)
                {
                    Logger.LogError("Failed to load service provider from assembly:" + assemblyFile);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return false;
            }
        }

        public string Name
        {
            get { return Config.Name; }
        }

        private ISocketServer m_SocketServer;

        public virtual bool Start()
        {
            if (this.IsRunning)
            {
                Logger.LogError("This socket server is running already, you needn't start it.");
                return false;
            }

            if (!m_SocketServer.Start())
                return false;

            StartSessionSnapshotTimer();

            if (Config.ClearIdleSession)
                StartClearSessionTimer();

            if (!StartConsoleHost())
            {
                Logger.LogError("Failed to start console service host!");
                Stop();
                return false;
            }

            return true;
        }

        public virtual void Stop()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsRunning
        {
            get
            {
                if (m_SocketServer == null)
                    return false;

                return m_SocketServer.IsRunning;
            }
        }

        public virtual bool IsReady
        {
            get { return true; }
        }

        private CommandHandler<TAppSession, TCommandInfo> m_CommandHandler;

        public event CommandHandler<TAppSession, TCommandInfo> CommandHandler
        {
            add { m_CommandHandler += value; }
            remove { m_CommandHandler -= value; }
        }

        public virtual void ExecuteCommand(TAppSession session, TCommandInfo commandInfo)
        {
            if (m_CommandHandler == null)
            {
                var command = GetCommandByName(commandInfo.Key);

                if (command != null)
                {
                    session.Context.CurrentCommand = commandInfo.Key;
                    command.ExecuteCommand(session, commandInfo);
                    session.Context.PrevCommand = commandInfo.Key;

                    if (Config.LogCommand)
                        Logger.LogInfo(session, string.Format("Command - {0}", commandInfo.Key));
                }
                else
                {
                    session.HandleUnknownCommand(commandInfo);
                }

                session.LastActiveTime = DateTime.Now;
            }
            else
            {
                session.Context.CurrentCommand = commandInfo.Key;
                m_CommandHandler(session, commandInfo);
                session.Context.PrevCommand = commandInfo.Key;
                session.LastActiveTime = DateTime.Now;

                if (Config.LogCommand)
                    Logger.LogInfo(session, string.Format("Command - {0}", commandInfo.Key));
            }
        }

        private ConcurrentDictionary<string, TAppSession> m_SessionDict = new ConcurrentDictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);

        public TAppSession CreateAppSession(ISocketSession socketSession)
        {
            TAppSession appSession = new TAppSession();
            appSession.Initialize(this, socketSession);
            socketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(OnSocketSessionClosed);

            if (m_SessionDict.TryAdd(appSession.IdentityKey, appSession))
            {
                Logger.LogInfo(appSession, "New SocketSession was accepted!");
                return appSession;
            }
            else
            {
                Logger.LogError(appSession, "SocketSession was refused because the session's IdentityKey already exists!");
                return default(TAppSession);
            }
        }

        public TAppSession GetAppSessionByIndentityKey(string identityKey)
        {
            TAppSession targetSession;
            m_SessionDict.TryGetValue(identityKey, out targetSession);
            return targetSession;
        }

        protected virtual void OnSocketSessionClosed(object sender, SocketSessionClosedEventArgs e)
        {
            //the sender is a sessionID
            string identityKey = e.IdentityKey;

            if (string.IsNullOrEmpty(identityKey))
                return;

            TAppSession removedSession;
            if (m_SessionDict.TryRemove(identityKey, out removedSession))
                Logger.LogInfo(removedSession, "This session was closed!");
            else
                Logger.LogError(removedSession, "Failed to remove this session, Because it haven't been in session container!");
        }

        public int SessionCount
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

        private void ClearIdleSession(object state)
        {
            if (Monitor.TryEnter(state))
            {
                try
                {
                    DateTime now = DateTime.Now;
                    DateTime timeOut = now.AddSeconds(0 - Config.IdleSessionTimeOut);

                    var timeOutSessions = m_SessionsSnapshot.Where(s => s.Value.LastActiveTime <= timeOut).Select(s => s.Value);
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

        public ICommand<TAppSession, TCommandInfo> GetCommandByName(string commandName)
        {
            ICommand<TAppSession, TCommandInfo> command;

            if (m_CommandDict.TryGetValue(commandName, out command))
                return command;
            else
                return null;
        }

        #endregion

        #region Take session snapshot

        private System.Threading.Timer m_SessionSnapshotTimer = null;

        private KeyValuePair<string, TAppSession>[] m_SessionsSnapshot = new KeyValuePair<string, TAppSession>[0];

        private void StartSessionSnapshotTimer()
        {
            int interval = Math.Max(Config.SessionSnapshotInterval, 1) * 1000;//in milliseconds
            m_SessionSnapshotTimer = new System.Threading.Timer(ClearIdleSession, new object(), interval, interval);
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
        public IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
        {
            return m_SessionsSnapshot.Select(p => p.Value).Where(critera);
        }

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TAppSession> GetAllSessions()
        {
            return m_SessionsSnapshot.Select(p => p.Value);
        }

        #endregion

        public void LogPerf()
        {
            Logger.LogPerf(string.Format("Live Connection Count: {0}", m_SessionDict.Count));
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                {
                    m_SocketServer.Stop();
                }

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

                CloseConsoleHost();
            }
        }

        #endregion
    }
}
