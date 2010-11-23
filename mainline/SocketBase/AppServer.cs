using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Security;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    public abstract class AppServer<TAppSession> : AppServer<TAppSession, StringCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringCommandInfo>, new()
    {

    }

    public abstract class AppServer<TAppSession, TCommandInfo> : IAppServer<TAppSession, TCommandInfo>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TAppSession, TCommandInfo>, new()
    {
        private IPEndPoint m_LocalEndPoint;

        public IServerConfig Config { get; private set; }

        protected virtual ConsoleHostInfo ConsoleHostInfo { get { return null; } }

        public virtual X509Certificate Certificate { get; protected set; }

        public virtual object Protocol { get; protected set; }

        private string m_ConsoleBaseAddress;

        public ServiceCredentials ServerCredentials { get; set; }

        private Dictionary<string, ICommand<TAppSession, TCommandInfo>> m_CommandDict = new Dictionary<string, ICommand<TAppSession, TCommandInfo>>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, ProviderBase<TAppSession>> m_ProviderDict = new Dictionary<string, ProviderBase<TAppSession>>(StringComparer.OrdinalIgnoreCase);

        private ICommandLoader m_CommandLoader = new ReflectCommandLoader();

        private ISocketServerFactory m_SocketServerFactory;

        public AppServer()
        {
            
        }       

        private bool LoadCommands()
        {
            foreach (var command in m_CommandLoader.LoadCommands<TAppSession, TCommandInfo>())
            {
                //command.DefaultParameterParser = CommandParameterParser;
                if (m_CommandDict.ContainsKey(command.Name))
                {
                    LogUtil.LogError(this, "Duplicated name command has been found! Command name: " + command.Name);
                    return false;
                }

                m_CommandDict.Add(command.Name, command);
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
                LogUtil.LogError(this, e);
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
                LogUtil.LogError(this, "Failed to close console service host for " + Name, e);
            }
            finally
            {
                m_ConsoleHost = null;
            }
        }

        public bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory)
        {
            return Setup(config, socketServerFactory, null);
        }

        public bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory, object protocol)
        {
            return Setup(config, socketServerFactory, protocol, string.Empty);
        }

        public bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory, object protocol, string consoleBaseAddress)
        {
            return Setup(config, socketServerFactory, protocol, consoleBaseAddress, string.Empty);
        }

        public virtual bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory, object protocol, string consoleBaseAddress, string assembly)
        {
            Config = config;
            m_ConsoleBaseAddress = consoleBaseAddress;
            m_SocketServerFactory = socketServerFactory;

            if (!SetupLocalEndpoint(config))
            {
                LogUtil.LogError(this, "Invalid config ip/port");
                return false;
            }

            //The protocol passed from config has higher priority
            if (protocol != null)
                Protocol = protocol;

            //If there is no defined protocol, use CommandLineProtocol as default
            if (Protocol == null)
                Protocol = new CommandLineProtocol();

            if (!LoadCommands())
                return false;

            if (!string.IsNullOrEmpty(assembly))
            {
                if (!SetupServiceProviders(config, assembly))
                    return false;
            }

            if (config.Certificate != null
                && config.Certificate.IsEnabled)
            {
                if (!SetupCertificate(config))
                    return false;
            }

            return SetupSocketServer();
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
                LogUtil.LogError(this, e);
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
                    else
                        m_LocalEndPoint = new IPEndPoint(IPAddress.Parse(config.Ip), config.Port);

                    return true;
                }
                catch (Exception)
                {
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
                LogUtil.LogError(this, "Failed to initialize certificate!", e);
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
                        LogUtil.LogError(this, "Failed to initalize provider " + providerType.ToString() + "!");
                        return false;
                    }
                }

                if (!IsReady)
                {
                    LogUtil.LogError(this, "Failed to load service provider from assembly:" + assemblyFile);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(this, e);
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
                LogUtil.LogError(this, "This socket server is running already, you needn't start it.");
                return false;
            }

            if (!m_SocketServer.Start())
                return false;

            if (Config.ClearIdleSession)
                StartClearSessionTimer();

            if (!StartConsoleHost())
            {
                LogUtil.LogError(this, "Failed to start console service host for " + Name);
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
                var command = GetCommandByName(commandInfo.CommandKey);

                if (command != null)
                {
                    session.Context.CurrentCommand = commandInfo.CommandKey;
                    command.ExecuteCommand(session, commandInfo);
                    session.Context.PrevCommand = commandInfo.CommandKey;

                    if (Config.LogCommand)
                        LogUtil.LogInfo(this, string.Format("Command - {0} - {1}", session.IdentityKey, commandInfo.CommandKey));
                }
                else
                {
                    session.HandleUnknownCommand(commandInfo);
                }

                session.LastActiveTime = DateTime.Now;
            }
            else
            {
                session.Context.CurrentCommand = commandInfo.CommandKey;
                m_CommandHandler(session, commandInfo);
                session.Context.PrevCommand = commandInfo.CommandKey;                
                session.LastActiveTime = DateTime.Now;

                if (Config.LogCommand)
                    LogUtil.LogInfo(this, string.Format("Command - {0} - {1}", session.IdentityKey, commandInfo.CommandKey));
            }
        }

        private Dictionary<string, TAppSession> m_SessionDict = new Dictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);

        private object m_SessionSyncRoot = new object();

        public TAppSession CreateAppSession(ISocketSession socketSession)
        {
            TAppSession appSession = new TAppSession();
            appSession.Initialize(this, socketSession);
            socketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(OnSocketSessionClosed);

            lock (m_SessionSyncRoot)
            {
                m_SessionDict[appSession.IdentityKey] = appSession;
            }

            LogUtil.LogInfo(this, "SocketSession " + socketSession.IdentityKey + " was accepted!");
            return appSession;
        }

        public TAppSession GetAppSessionByIndentityKey(string identityKey)
        {
            TAppSession targetSession;

            lock (m_SessionSyncRoot)
            {
                m_SessionDict.TryGetValue(identityKey, out targetSession);
                return targetSession;
            }
        }

        protected virtual void OnSocketSessionClosed(object sender, SocketSessionClosedEventArgs e)
        {
            //the sender is a sessionID
            string identityKey = e.IdentityKey;

            if (string.IsNullOrEmpty(identityKey))
                return;

            try
            {
                lock (m_SessionSyncRoot)
                {
                    m_SessionDict.Remove(identityKey);
                }
                LogUtil.LogInfo(this, "SocketSession " + identityKey + " was closed!");
            }
            catch (Exception exc)
            {
                LogUtil.LogError(this, exc);
            }
        }

        public int SessionCount
        {
            get
            {
                return m_SessionDict.Count;
            }
        }

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
                    lock (m_SessionSyncRoot)
                    {
                        m_SessionDict.Values.Where(s =>
                            DateTime.Now.Subtract(s.LastActiveTime).TotalSeconds > Config.IdleSessionTimeOut)
                            .ToList().ForEach(s =>
                            {
                                s.Close();
                                LogUtil.LogInfo(this, string.Format("The socket session: {0} has been closed for {1} timeout!", s.IdentityKey, DateTime.Now.Subtract(s.LastActiveTime).TotalSeconds));
                            });
                    }
                }
                catch (Exception e)
                {
                    LogUtil.LogError(this, "Clear idle session error!", e);
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

                if (m_ClearIdleSessionTimer != null)
                {
                    m_ClearIdleSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_ClearIdleSessionTimer.Dispose();
                    m_ClearIdleSessionTimer = null;
                }

                m_SessionDict.Values.ToList().ForEach(s => s.Close());

                CloseConsoleHost();
            }
        }

        #endregion
    }
}
