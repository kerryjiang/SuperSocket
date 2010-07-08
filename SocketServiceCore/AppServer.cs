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
        T CreateAppSession(ISocketSession socketSession);
    }

    public abstract class AppServer<T> : IAppServer<T> where T : IAppSession, IAppSession<T>, new()
    {
        private IPEndPoint m_LocalEndPoint;

        public IServerConfig Config { get; private set; }

        protected virtual ConsoleHostInfo ConsoleHostInfo { get { return null; } }

        private string m_ConsoleBaseAddress;

        public AppServer()
		{
            LoadCommands();
		}

        public ServiceCredentials ServerCredentials { get; set; }

        private Dictionary<string, ICommand<T>> dictCommand = new Dictionary<string, ICommand<T>>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Loads the command dictionary.
        /// </summary>
        private void LoadCommands()
        {
            Type commandType = typeof(ICommand<T>);
            Assembly asm = typeof(T).Assembly;
            Type[] arrType = asm.GetExportedTypes();

            for (int i = 0; i < arrType.Length; i++)
            {
                //LogUtil.LogInfo(arrType[i].ToString() + "\r\n");
                var commandInterface = arrType[i].GetInterfaces().SingleOrDefault(x => x == commandType);

                if (commandInterface != null)
                {
                    //LogUtil.LogInfo(arrInterface[j].ToString() + ":" + commandType.ToString());
                    dictCommand[arrType[i].Name] = arrType[i].GetConstructor(new Type[0]).Invoke(new object[0]) as ICommand<T>;
                }
            }
            //LogUtil.LogInfo("Load " + dictCommand.Count + " commands from " + arrType.Length + " types!");
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

        private Dictionary<string, ProviderBase> m_ProviderDict = new Dictionary<string, ProviderBase>(StringComparer.OrdinalIgnoreCase);

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

            if (config.Port > 0)
            {
                try
                {
                    if(string.IsNullOrEmpty(config.Ip) || "Any".Equals(config.Ip, StringComparison.OrdinalIgnoreCase))
                        m_LocalEndPoint = new IPEndPoint(IPAddress.Any, config.Port);
                    else
                        m_LocalEndPoint = new IPEndPoint(IPAddress.Parse(config.Ip), config.Port);
                }
                catch(Exception e)
                {
                    LogUtil.LogError("Invalid config ip/port", e);
                    return false;
                }
            }

            if (m_LocalEndPoint == null)
            {
                LogUtil.LogError("Config ip/port is missing!");
                return false;
            }

            if (string.IsNullOrEmpty(assembly))
                return true;

            string dir = FileHelper.GetParentFolder(this.GetType().Assembly.Location);

            string assemblyFile = Path.Combine(dir, assembly + ".dll");

            try
            {
                Type typeProvider = typeof(ProviderBase);

                Assembly ass = Assembly.LoadFrom(assemblyFile);

                Type[] arrType = ass.GetTypes();

                for (int i = 0; arrType != null && i < arrType.Length; i++)
                {
                    //Must be a seal class
                    if (arrType[i].IsSubclassOf(typeProvider))
                    {
                        ProviderBase provider = ass.CreateInstance(arrType[i].ToString()) as ProviderBase;
                        if (provider.Init(config))
                        {
                            m_ProviderDict[provider.Name] = provider;
                        }
                        else
                        {
                            LogUtil.LogError("Failed to initalize provider " + arrType[i].ToString() + "!");
                            return false;
                        }
                    }
                }

                if (!IsReady)
                {
                    LogUtil.LogError("Failed to load service provider from assembly:" + assemblyFile);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
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

            SetupClearSessionTimer();

            if (!StartConsoleHost())
            {
                LogUtil.LogError("Failed to start console service host for " + Name);
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
                LogUtil.LogError(e);
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

            m_ClearIdleSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_ClearIdleSessionTimer.Dispose();

            m_SessionDict.Values.ToList().ForEach(s => s.Close());

            if (m_ConsoleHost != null)
            {
                try
                {
                    m_ConsoleHost.Close();
                }
                catch (Exception e)
                {
                    LogUtil.LogError("Failed to close console service host for " + Name, e);
                }
                finally
                {
                    m_ConsoleHost = null;
                }
            }
        }

        /// <summary>
        /// Gets service provider by name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        protected ProviderBase GetProviderByName(string providerName)
        {
            ProviderBase provider = null;

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
                LogUtil.LogInfo("SocketSession " + sessionID + " was closed!");
            }
            catch (Exception exc)
            {
                LogUtil.LogError(exc);
            }
        }

        private System.Threading.Timer m_ClearIdleSessionTimer = null;

        private void SetupClearSessionTimer()
        {
            int interval  = 60 * 1000;
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
                            DateTime.Now.Subtract(s.SocketSession.LastActiveTime).TotalMinutes > 5)
                            .ToList().ForEach(s => s.Close());
                    }
                }
                catch (Exception e)
                {
                    LogUtil.LogError("Clear idle session error!", e);
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
    }
}
