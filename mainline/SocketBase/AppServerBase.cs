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
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Security;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// AppServer base class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class AppServerBase<TAppSession, TRequestInfo> : IAppServer<TAppSession, TRequestInfo>, ICommandSource<ICommand<TAppSession, TRequestInfo>>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession<TAppSession, TRequestInfo>, new()
    {
        /// <summary>
        /// Null appSession instance
        /// </summary>
        protected readonly TAppSession NullAppSession = default(TAppSession);

        /// <summary>
        /// Gets the server's config.
        /// </summary>
        public IServerConfig Config { get; private set; }

        /// <summary>
        /// Gets the certificate of current server.
        /// </summary>
        public virtual X509Certificate Certificate { get; protected set; }

        /// <summary>
        /// Gets or sets the request filter factory.
        /// </summary>
        /// <value>
        /// The request filter factory.
        /// </value>
        public virtual IRequestFilterFactory<TRequestInfo> RequestFilterFactory { get; protected set; }

        private List<ICommandLoader> m_CommandLoaders;

        private Dictionary<string, ICommand<TAppSession, TRequestInfo>> m_CommandDict = new Dictionary<string, ICommand<TAppSession, TRequestInfo>>(StringComparer.OrdinalIgnoreCase);

        private ISocketServerFactory m_SocketServerFactory;

        /// <summary>
        /// Gets the basic transfer layer security protocol.
        /// </summary>
        public SslProtocols BasicSecurity { get; private set; }

        /// <summary>
        /// Gets the root config.
        /// </summary>
        protected IRootConfig RootConfig { get; private set; }

        /// <summary>
        /// Gets the logger assosiated with this object.
        /// </summary>
        public ILog Logger { get; private set; }

        private static bool m_ThreadPoolConfigured = false;

        private List<IConnectionFilter> m_ConnectionFilters;

        private Dictionary<string, List<CommandFilterAttribute>> m_CommandFilterDict;

        private long m_TotalHandledRequests = 0;

        /// <summary>
        /// Gets the total handled requests number.
        /// </summary>
        protected long TotalHandledRequests
        {
            get { return m_TotalHandledRequests; }
        }

        private ListenerInfo[] m_Listeners;

        /// <summary>
        /// Gets or sets the listeners inforamtion.
        /// </summary>
        /// <value>
        /// The listeners.
        /// </value>
        public ListenerInfo[] Listeners
        {
            get { return m_Listeners; }
        }

        /// <summary>
        /// Gets the started time of this server instance.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        public DateTime StartedTime { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServerBase&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        public AppServerBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServerBase&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="requestFilterFactory">The request filter factory.</param>
        public AppServerBase(IRequestFilterFactory<TRequestInfo> requestFilterFactory)
        {
            this.RequestFilterFactory = requestFilterFactory;
        }


        /// <summary>
        /// Setups the command into command dictionary
        /// </summary>
        /// <param name="commandDict">The target command dict.</param>
        /// <returns></returns>
        protected virtual bool SetupCommands(Dictionary<string, ICommand<TAppSession, TRequestInfo>> commandDict)
        {
            foreach (var loader in m_CommandLoaders)
            {
                try
                {
                    var ret = loader.LoadCommands<TAppSession, TRequestInfo>(this, c =>
                    {
                        if (commandDict.ContainsKey(c.Name))
                        {
                            if (Logger.IsErrorEnabled)
                                Logger.Error("Duplicated name command has been found! Command name: " + c.Name);
                            return false;
                        }

                        commandDict.Add(c.Name, c);
                        return true;
                    }, u =>
                    {
                        var workingDict = m_CommandDict.Values.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
                        var updatedCommands = 0;

                        foreach (var c in u)
                        {
                            if (c == null)
                                continue;

                            if (c.UpdateAction == CommandUpdateAction.Remove)
                            {
                                workingDict.Remove(c.Command.Name);
                                if (Logger.IsInfoEnabled)
                                    Logger.InfoFormat("The command '{0}' has been removed from this server!", c.Command.Name);
                            }
                            else if (c.UpdateAction == CommandUpdateAction.Add)
                            {
                                workingDict.Add(c.Command.Name, c.Command);
                                if (Logger.IsInfoEnabled)
                                    Logger.InfoFormat("The command '{0}' has been added into this server!", c.Command.Name);
                            }
                            else
                            {
                                workingDict[c.Command.Name] = c.Command;
                                if (Logger.IsInfoEnabled)
                                    Logger.InfoFormat("The command '{0}' has been updated!", c.Command.Name);
                            }

                            updatedCommands++;
                        }

                        if (updatedCommands > 0)
                        {
                            Interlocked.Exchange<Dictionary<string, ICommand<TAppSession, TRequestInfo>>>(ref m_CommandDict, workingDict);
                        }
                    });

                    if (!ret)
                        return false;
                }
                catch (Exception e)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("Failed to load command by " + loader.GetType().ToString() + "!", e);
                    return false;
                }
            }

            SetupCommandFilters(commandDict.Values);

            return true;
        }

        /// <summary>
        /// Setups the command filters.
        /// </summary>
        /// <param name="commands">The commands.</param>
        private void SetupCommandFilters(IEnumerable<ICommand<TAppSession, TRequestInfo>> commands)
        {
            m_CommandFilterDict = CommandFilterFactory.GenerateCommandFilterLibrary(this.GetType(), commands.Cast<ICommand>());
        }

        /// <summary>
        /// Setups the appServer instance
        /// </summary>
        /// <param name="rootConfig">The SuperSocket root config.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <returns></returns>
        public bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory)
        {
            return Setup(rootConfig, config, socketServerFactory, null);
        }

        /// <summary>
        /// Setups the appServer instance
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="requestFilterFactory">The request filter factory.</param>
        /// <returns></returns>
        public virtual bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, IRequestFilterFactory<TRequestInfo> requestFilterFactory)
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

            if (!(config is ServerConfig))
            {
                //Use config plain model directly to avoid extra object casting in runtime
                var newConfig = new ServerConfig();
                config.CopyPropertiesTo(newConfig);
                config = newConfig;
            }

            Config = config;

            m_SocketServerFactory = socketServerFactory;

            SetupLogger();

            if (!SetupSecurity(config))
                return false;

            if (!SetupListeners(config))
            {
                if(Logger.IsErrorEnabled)
                    Logger.Error("Invalid config ip/port");

                return false;
            }

            if (!SetupRequestFilterFactory(config, requestFilterFactory))
                return false;

            m_CommandLoaders = new List<ICommandLoader>
            {
                new ReflectCommandLoader()
            };

            if (Config.EnableDynamicCommand)
            {
                ICommandLoader dynamicCommandLoader;

                try
                {
                    dynamicCommandLoader = AssemblyUtil.CreateInstance<ICommandLoader>("SuperSocket.Dlr.DynamicCommandLoader, SuperSocket.Dlr");
                }
                catch (Exception e)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("The file SuperSocket.Dlr is required for dynamic command support!", e);

                    return false;
                }

                m_CommandLoaders.Add(dynamicCommandLoader);
            }

            if (!SetupCommands(m_CommandDict))
                return false;

            return SetupSocketServer();
        }

        /// <summary>
        /// Setups the request filter factory.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="requestFilterFactory">The request filter factory.</param>
        /// <returns></returns>
        private bool SetupRequestFilterFactory(IServerConfig config, IRequestFilterFactory<TRequestInfo> requestFilterFactory)
        {
            //The protocol passed by programming has higher priority, then by config
            if (requestFilterFactory != null)
            {
                this.RequestFilterFactory = requestFilterFactory;
            }
            else
            {
                //There is a protocol configuration existing
                if (!string.IsNullOrEmpty(config.Protocol))
                {
                    IRequestFilterFactory<TRequestInfo> configuredRequestFilterFactory;

                    try
                    {
                        configuredRequestFilterFactory = AssemblyUtil.CreateInstance<IRequestFilterFactory<TRequestInfo>>(config.Protocol);
                    }
                    catch(Exception e)
                    {
                        if (Logger.IsErrorEnabled)
                            Logger.Error(string.Format("Invalid configured protocol {0}.", config.Protocol), e);

                        return false;
                    }

                    this.RequestFilterFactory = configuredRequestFilterFactory;
                }
            }

            //If there is no defined protocol, use CommandLineProtocol as default
            if (RequestFilterFactory == null)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("Protocol hasn't been set!");

                return false;
            }

            return true;
        }

        private void SetupLogger()
        {
            Logger = LogFactoryProvider.LogFactory.GetLog(this.Name);
        }

        /// <summary>
        /// Setups the security option of socket communications.
        /// </summary>
        /// <param name="config">The config of the server instance.</param>
        /// <returns></returns>
        private bool SetupSecurity(IServerConfig config)
        {
            if (!string.IsNullOrEmpty(config.Security))
            {
                SslProtocols configProtocol;
                if (!config.Security.TryParseEnum<SslProtocols>(true, out configProtocol))
                {
                    if (Logger.IsErrorEnabled)
                        Logger.ErrorFormat("Failed to parse '{0}' to SslProtocol!", config.Security);

                    return false;
                }

                if (configProtocol != SslProtocols.None)
                {
                    if (config.Certificate == null || !config.Certificate.IsEnabled)
                    {
                        if (Logger.IsErrorEnabled)
                            Logger.Error("There is no certificate defined and enabled!");
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

        /// <summary>
        /// Setups the socket server.instance
        /// </summary>
        /// <returns></returns>
        private bool SetupSocketServer()
        {
            try
            {
                m_SocketServer = m_SocketServerFactory.CreateSocketServer<TRequestInfo>(this, m_Listeners, Config, RequestFilterFactory);
                return m_SocketServer != null;
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(e);

                return false;
            }
        }

        private IPAddress ParseIPAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip) || "Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                return IPAddress.Any;
            else if ("IPv6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                return IPAddress.IPv6Any;
            else
               return IPAddress.Parse(ip);
        }

        /// <summary>
        /// Setups the listeners base on server configuration
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        private bool SetupListeners(IServerConfig config)
        {
            var listeners = new List<ListenerInfo>();

            try
            {
                if (config.Port > 0)
                {
                    listeners.Add(new ListenerInfo
                    {
                        EndPoint = new IPEndPoint(ParseIPAddress(config.Ip), config.Port),
                        BackLog = config.ListenBacklog,
                        Security = BasicSecurity
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(config.Ip))
                    {
                        if (Logger.IsErrorEnabled)
                            Logger.Error("Ip is required in config!");

                        return false;
                    }
                }

                //There are listener defined
                if (config.Listeners != null && config.Listeners.Any())
                {
                    //But ip and port were configured in server node
                    //We don't allow this case
                    if (listeners.Count > 0)
                    {
                        if (Logger.IsErrorEnabled)
                            Logger.Error("If you configured Ip and Port in server node, you cannot defined listeners any more!");

                        return false;
                    }

                    foreach (var l in config.Listeners)
                    {
                        SslProtocols configProtocol;

                        if (string.IsNullOrEmpty(l.Security) && BasicSecurity != SslProtocols.None)
                        {
                            configProtocol = BasicSecurity;
                        }
                        else if (!l.Security.TryParseEnum<SslProtocols>(true, out configProtocol))
                        {
                            if (Logger.IsErrorEnabled)
                                Logger.ErrorFormat("Failed to parse '{0}' to SslProtocol!", config.Security);

                            return false;
                        }

                        if (configProtocol != SslProtocols.None && (config.Certificate == null || !config.Certificate.IsEnabled))
                        {
                            if (Logger.IsErrorEnabled)
                                Logger.Error("There is no certificate defined and enabled!");
                            return false;
                        }

                        listeners.Add(new ListenerInfo
                        {
                            EndPoint = new IPEndPoint(ParseIPAddress(l.Ip), l.Port),
                            BackLog = l.Backlog,
                            Security = configProtocol
                        });
                    }
                }

                if (!listeners.Any())
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("No listener defined!");
                }

                m_Listeners = listeners.ToArray();

                return true;
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(e);

                return false;
            }
        }

        /// <summary>
        /// Setups the TLS/SSL certificate.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        private bool SetupCertificate(IServerConfig config)
        {
            try
            {
                if (config.Certificate.IsEnabled && string.IsNullOrEmpty(config.Certificate.FilePath) && string.IsNullOrEmpty(config.Certificate.Thumbprint))
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("Failed to initialize certificate! The attributes 'filePath' or 'thumbprint' is required!");

                    return false;
                }

                Certificate = CertificateManager.Initialize(config.Certificate);
                return true;
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("Failed to initialize certificate!", e);

                return false;
            }
        }


        /// <summary>
        /// Gets the name of the server instance.
        /// </summary>
        public string Name
        {
            get { return Config.Name; }
        }

        private ISocketServer m_SocketServer;

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>
        /// return true if start successfull, else false
        /// </returns>
        public virtual bool Start()
        {
            if (this.IsRunning)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("This socket server is running already, you needn't start it.");

                return false;
            }

            if (!m_SocketServer.Start())
                return false;

            StartedTime = DateTime.Now;

            OnStartup();

            return true;
        }

        /// <summary>
        /// Called when [startup].
        /// </summary>
        protected virtual void OnStartup()
        {

        }

        /// <summary>
        /// Called when [stopped].
        /// </summary>
        protected virtual void OnStopped()
        {

        }

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        public virtual void Stop()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            OnStopped();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning
        {
            get
            {
                if (m_SocketServer == null)
                    return false;

                return m_SocketServer.IsRunning;
            }
        }

        /// <summary>
        /// Gets command by command name.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <returns></returns>
        public ICommand<TAppSession, TRequestInfo> GetCommandByName(string commandName)
        {
            ICommand<TAppSession, TRequestInfo> command;

            if (m_CommandDict.TryGetValue(commandName, out command))
                return command;
            else
                return null;
        }

        private CommandHandler<TAppSession, TRequestInfo> m_CommandHandler;

        /// <summary>
        /// Occurs when a full request item received.
        /// </summary>
        protected event CommandHandler<TAppSession, TRequestInfo> CommandHandler
        {
            add { m_CommandHandler += value; }
            remove { m_CommandHandler -= value; }
        }

        /// <summary>
        /// Executes the command filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="session">The session.</param>
        /// <param name="command">The command.</param>
        /// <param name="filterAction">The filter action.</param>
        private void ExecuteCommandFilters(List<CommandFilterAttribute> filters, TAppSession session, ICommand command, Action<CommandFilterAttribute, TAppSession, ICommand> filterAction)
        {
            if (filters == null || filters.Count <= 0)
                return;

            for (var i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                filterAction(filter, session, command);
            }
        }

        private Action<CommandFilterAttribute, TAppSession, ICommand> m_CommandFilterExecutingAction = (f, s, c) => f.OnCommandExecuting(s, c);

        private Action<CommandFilterAttribute, TAppSession, ICommand> m_CommandFilterExecutedAction = (f, s, c) => f.OnCommandExecuted(s, c);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        protected virtual void ExecuteCommand(TAppSession session, TRequestInfo requestInfo)
        {
            if (m_CommandHandler == null)
            {
                var command = GetCommandByName(requestInfo.Key);

                if (command != null)
                {
                    List<CommandFilterAttribute> commandFilters = null;

                    if (m_CommandFilterDict != null)
                        m_CommandFilterDict.TryGetValue(command.Name, out commandFilters);

                    session.CurrentCommand = requestInfo.Key;

                    if (commandFilters != null)
                        ExecuteCommandFilters(commandFilters, session, command, m_CommandFilterExecutingAction);

                    //Command filter may close the session,
                    //so detect whether session is connected before execute command
                    if (session.Status != SessionStatus.Disconnected)
                    {
                        command.ExecuteCommand(session, requestInfo);

                        if (commandFilters != null)
                            ExecuteCommandFilters(commandFilters, session, command, m_CommandFilterExecutedAction);
                    }

                    session.PrevCommand = requestInfo.Key;

                    if (Config.LogCommand && Logger.IsInfoEnabled)
                        Logger.Info(session, string.Format("Command - {0}", requestInfo.Key));
                }
                else
                {
                    session.HandleUnknownRequest(requestInfo);
                }

                session.LastActiveTime = DateTime.Now;
            }
            else
            {
                session.CurrentCommand = requestInfo.Key;
                m_CommandHandler(session, requestInfo);
                session.PrevCommand = requestInfo.Key;
                session.LastActiveTime = DateTime.Now;

                if (Config.LogCommand && Logger.IsInfoEnabled)
                    Logger.Info(session, string.Format("Command - {0}", requestInfo.Key));
            }

            Interlocked.Increment(ref m_TotalHandledRequests);
        }

        /// <summary>
        /// Executes the command for the session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        void IAppServer<TAppSession, TRequestInfo>.ExecuteCommand(IAppSession<TRequestInfo> session, TRequestInfo requestInfo)
        {
            this.ExecuteCommand((TAppSession)session, requestInfo);
        }

        /// <summary>
        /// Gets or sets the server's connection filter
        /// </summary>
        /// <value>
        /// The server's connection filters
        /// </value>
        public IEnumerable<IConnectionFilter> ConnectionFilters
        {
            get { return m_ConnectionFilters; }
            set
            {
                if (m_ConnectionFilters == null)
                    m_ConnectionFilters = new List<IConnectionFilter>();

                m_ConnectionFilters.AddRange(value);
            }
        }

        /// <summary>
        /// Executes the connection filters.
        /// </summary>
        /// <param name="remoteAddress">The remote address.</param>
        /// <returns></returns>
        private bool ExecuteConnectionFilters(IPEndPoint remoteAddress)
        {
            if (m_ConnectionFilters == null)
                return true;

            for (var i = 0; i < m_ConnectionFilters.Count; i++)
            {
                var currentFilter = m_ConnectionFilters[i];
                if (!currentFilter.AllowConnect(remoteAddress))
                {
                    if (Logger.IsInfoEnabled)
                        Logger.InfoFormat("A connection from {0} has been refused by filter {1}!", remoteAddress, currentFilter.Name);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        public virtual IAppSession CreateAppSession(ISocketSession socketSession)
        {
            if (!ExecuteConnectionFilters(socketSession.RemoteEndPoint))
                return NullAppSession;

            var appSession = new TAppSession();
            appSession.Initialize(this, socketSession, RequestFilterFactory.CreateFilter(this, socketSession));
            socketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(OnSocketSessionClosed);

            return appSession;
        }

        /// <summary>
        /// Resets the session's security protocol.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="security">The security protocol.</param>
        public void ResetSessionSecurity(IAppSession session, SslProtocols security)
        {
            m_SocketServer.ResetSessionSecurity(session, security);
        }

        /// <summary>
        /// Called when [socket session closed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SuperSocket.SocketBase.SocketSessionClosedEventArgs"/> instance containing the event data.</param>
        internal protected virtual void OnSocketSessionClosed(object sender, SocketSessionClosedEventArgs e)
        {

        }

        /// <summary>
        /// Gets the app session by ID internal.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        protected abstract IAppSession GetAppSessionByIDInternal(string sessionID);

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        IAppSession IAppServer.GetAppSessionByID(string sessionID)
        {
            return GetAppSessionByIDInternal(sessionID);
        }

        /// <summary>
        /// Gets the matched sessions from sessions snapshot.
        /// </summary>
        /// <param name="critera">The prediction critera.</param>
        public virtual IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        public virtual IEnumerable<TAppSession> GetAllSessions()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        public virtual int SessionCount
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the service instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetService<T>()
            where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                {
                    m_SocketServer.Stop();
                }
            }
        }

        #endregion
    }
}
