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
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.SocketServiceCore.Security;

namespace SuperSocket.SocketServiceCore
{
    public interface IAppServer<T> : IRunable<T>, ICommandSource<T>
         where T : IAppSession, new()
    {
        IServerConfig Config { get; }
        ICommandParser CommandParser { get; }
        X509Certificate Certificate { get; }
        T CreateAppSession(ISocketSession socketSession);
    }

    public abstract class AppServer<T> : IAppServer<T>, IDisposable
        where T : IAppSession, IAppSession<T>, new()
    {
        private IPEndPoint m_LocalEndPoint;

        public IServerConfig Config { get; private set; }

        protected virtual ConsoleHostInfo ConsoleHostInfo { get { return null; } }

        public virtual ICommandParser CommandParser { get; protected set; }

        public virtual ICommandParameterParser CommandParameterParser { get; protected set; }

        public virtual X509Certificate Certificate { get; protected set; }

        private string m_ConsoleBaseAddress;

        public AppServer()
        {
            
        }

        public ServiceCredentials ServerCredentials { get; set; }

        private Dictionary<string, ICommand<T>> m_CommandDict = new Dictionary<string, ICommand<T>>(StringComparer.OrdinalIgnoreCase);

        private ICommandLoader m_CommandLoader = new ReflectCommandLoader();

        private void LoadCommands()
        {
            foreach (var command in m_CommandLoader.LoadCommands<T>())
            {
                command.DefaultParameterParser = CommandParameterParser;
                m_CommandDict[command.GetType().Name] = command;
            }
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

        private Dictionary<string, ProviderBase<T>> m_ProviderDict = new Dictionary<string, ProviderBase<T>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Setups the specified factory.
        /// </summary>
        /// <param name="assembly">The assembly name.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public virtual bool Setup(string assembly, IServerConfig config, string consoleBaseAddress)
        {
            Config = config;
            m_ConsoleBaseAddress = consoleBaseAddress;

            if (!SetupLocalEndpoint(config))
            {
                LogUtil.LogError(this, "Invalid config ip/port");
                return false;
            }

            if (CommandParser == null)
                CommandParser = new BasicCommandParser();

            if (CommandParameterParser == null)
                CommandParameterParser = new SplitAllCommandParameterParser();

            LoadCommands();

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

            SetupSocketServer();

            return true;
        }

        private void SetupSocketServer()
        {
            if (Config.Mode == SocketMode.Sync)
            {
                m_SocketServer = new SyncSocketServer<SyncSocketSession<T>, T>(this, m_LocalEndPoint);
            }
            else
            {
                m_SocketServer = new AsyncSocketServer<AsyncSocketSession<T>, T>(this, m_LocalEndPoint);
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

        private bool SetupServiceProviders(IServerConfig config, string assembly)
        {
            string dir = Path.GetDirectoryName(this.GetType().Assembly.Location);

            string assemblyFile = Path.Combine(dir, assembly + ".dll");

            try
            {
                Assembly ass = Assembly.LoadFrom(assemblyFile);

                foreach(var providerType in ass.GetImplementTypes<ProviderBase<T>>())
                {
                    ProviderBase<T> provider = ass.CreateInstance(providerType.ToString()) as ProviderBase<T>;

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

        public string Name
        {
            get { return Config.Name; }
        }

        private ISocketServer m_SocketServer;

        public virtual bool Start()
        {
            if (!m_SocketServer.Start())
                return false;

            if(Config.ClearIdleSession)
                StartClearSessionTimer();

            if (!StartConsoleHost())
            {
                LogUtil.LogError(this, "Failed to start console service host for " + Name);
                Stop();
                return false;
            }

            return true;
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

        protected ProviderBase<T> GetProviderByName(string providerName)
        {
            ProviderBase<T> provider = null;

            if (m_ProviderDict.TryGetValue(providerName, out provider))
            {
                return provider;
            }
            else
            {
                return null;
            }
        }

        public virtual bool IsReady
        {
            get { return true; }
        }

        private Dictionary<string, T> m_SessionDict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        private object m_SessionSyncRoot = new object();

        public T CreateAppSession(ISocketSession socketSession)
        {
            T appSession = new T();
            appSession.Initialize(this, socketSession);
            socketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(socketSession_Closed);

            lock (m_SessionSyncRoot)
            {
                m_SessionDict[appSession.SessionID] = appSession;
            }

            return appSession;
        }

        void socketSession_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            //the sender is a sessionID
            string sessionID = e.SessionID;

            if (string.IsNullOrEmpty(sessionID))
                return;

            try
            {
                lock (m_SessionSyncRoot)
                {
                    m_SessionDict.Remove(sessionID);
                }
                LogUtil.LogInfo(this, "SocketSession " + sessionID + " was closed!");
            }
            catch (Exception exc)
            {
                LogUtil.LogError(this, exc);
            }
        }

        private System.Threading.Timer m_ClearIdleSessionTimer = null;

        private void StartClearSessionTimer()
        {
            int interval  = Config.ClearIdleSessionInterval * 1000;//in milliseconds
            m_ClearIdleSessionTimer = new System.Threading.Timer(ClearIdleSession, new object(), interval, interval);
        }

        private void ClearIdleSession(object state)
        {
            if(Monitor.TryEnter(state))
            {
                try
                {
                    lock (m_SessionSyncRoot)
                    {
                        m_SessionDict.Values.Where(s =>
                            DateTime.Now.Subtract(s.SocketSession.LastActiveTime).TotalMinutes > Config.IdleSessionTimeOut)
                            .ToList().ForEach(s => s.Close());
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

        #region ICommandSource<T> Members

        public ICommand<T> GetCommandByName(string commandName)
        {
            ICommand<T> command;

            if (m_CommandDict.TryGetValue(commandName, out command))
                return command;
            else
                return null;
        }

        #endregion

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
