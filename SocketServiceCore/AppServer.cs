using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace SuperSocket.SocketServiceCore
{
    public interface IAppServer<T> : IRunable<T>, ICommandSource<T>
         where T : IAppSession, new()
    {
        IServerConfig Config { get; }
        ICommandParser CommandParser { get; }
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

        private string m_ConsoleBaseAddress;

        public AppServer()
        {
            
        }

        public ServiceCredentials ServerCredentials { get; set; }

        private Dictionary<string, ICommand<T>> dictCommand = new Dictionary<string, ICommand<T>>(StringComparer.OrdinalIgnoreCase);

        private void LoadCommands()
        {
            Type commandType = typeof(ICommand<T>);
            Assembly asm = typeof(T).Assembly;
            Type[] arrType = asm.GetExportedTypes();

            for (int i = 0; i < arrType.Length; i++)
            {
                var commandInterface = arrType[i].GetInterfaces().SingleOrDefault(x => x == commandType);

                if (commandInterface != null)
                {
                    ICommand<T> command = arrType[i].GetConstructor(new Type[0]).Invoke(new object[0]) as ICommand<T>;
                    command.DefaultParameterParser = CommandParameterParser;
                    dictCommand[arrType[i].Name] = command;
                }
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

            return true;
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
            string dir = FileHelper.GetParentFolder(this.GetType().Assembly.Location);

            string assemblyFile = Path.Combine(dir, assembly + ".dll");

            try
            {
                Type typeProvider = typeof(ProviderBase<T>);

                Assembly ass = Assembly.LoadFrom(assemblyFile);

                Type[] arrType = ass.GetExportedTypes();

                for (int i = 0; arrType != null && i < arrType.Length; i++)
                {
                    //Must be a seal class
                    if (arrType[i].IsSubclassOf(typeProvider))
                    {
                        ProviderBase<T> provider = ass.CreateInstance(arrType[i].ToString()) as ProviderBase<T>;
                        if (provider.Init(this, config))
                        {
                            m_ProviderDict[provider.Name] = provider;
                        }
                        else
                        {
                            LogUtil.LogError(this, "Failed to initalize provider " + arrType[i].ToString() + "!");
                            return false;
                        }
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
            if (Config.Mode == SocketMode.Sync)
            {
                m_SocketServer = new SyncSocketServer<SyncSocketSession<T>, T>(this, m_LocalEndPoint);
                if (!m_SocketServer.Start())
                    return false;
            }
            else
            {
                m_SocketServer = new AsyncSocketServer<AsyncSocketSession<T>, T>(this, m_LocalEndPoint);
                if (!m_SocketServer.Start())
                    return false;
            }

            if(Config.ClearIdleSession)
                SetupClearSessionTimer();

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

        public virtual void Stop()
        {
            if (m_SocketServer != null)
            {
                m_SocketServer.Stop();
            }

            if (m_ClearIdleSessionTimer != null)
            {
                m_ClearIdleSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_ClearIdleSessionTimer.Dispose();
            }

            m_SessionDict.Values.ToList().ForEach(s => s.Close());

            if (m_ConsoleHost != null)
            {
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

            m_SocketServer = null;
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

        /// <summary>
        /// Gets service provider by name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
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

        private void SetupClearSessionTimer()
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
            }
        }

        #region ICommandSource<T> Members

        public ICommand<T> GetCommandByName(string commandName)
        {
            ICommand<T> command;

            if (dictCommand.TryGetValue(commandName, out command))
                return command;
            else
                return null;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (m_SocketServer != null)
            {
                if (m_SocketServer.IsRunning)
                    m_SocketServer.Stop();

                m_SocketServer = null;
            }
        }

        #endregion
    }
}
