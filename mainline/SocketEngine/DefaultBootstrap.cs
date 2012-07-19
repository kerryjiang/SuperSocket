using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketEngine.Configuration;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// SuperSocket default bootstrap
    /// </summary>
    public class DefaultBootstrap : IBootstrap
    {
        private List<IWorkItem> m_AppServers;

        /// <summary>
        /// Indicates whether the bootstrap is initialized
        /// </summary>
        private bool m_Initialized = false;

        /// <summary>
        /// Global configuration
        /// </summary>
        private IConfigurationSource m_Config;

        /// <summary>
        /// Global log
        /// </summary>
        private ILog m_GlobalLog;

        /// <summary>
        /// Gets the log factory.
        /// </summary>
        protected ILogFactory LogFactory { get; private set; }

        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        public IEnumerable<IWorkItem> AppServers
        {
            get { return m_AppServers; }
        }

        /// <summary>
        /// Gets the config.
        /// </summary>
        public IRootConfig Config
        {
            get { return m_Config; }
        }

        /// <summary>
        /// Gets the startup config file.
        /// </summary>
        public string StartupConfigFile { get; private set; }

        private PerformanceMonitor m_PerfMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public DefaultBootstrap(IConfigurationSource config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            var fileConfigSource = config as ConfigurationSection;

            if (fileConfigSource != null)
                StartupConfigFile = fileConfigSource.CurrentConfiguration.FilePath;

            m_Config = config;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="startupConfigFile">The startup config file.</param>
        public DefaultBootstrap(IConfigurationSource config, string startupConfigFile)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (!string.IsNullOrEmpty(startupConfigFile))
                StartupConfigFile = startupConfigFile;

            m_Config = config;
        }

        /// <summary>
        /// Creates the work item instance.
        /// </summary>
        /// <param name="serviceTypeName">Name of the service type.</param>
        /// <returns></returns>
        protected virtual IWorkItem CreateWorkItemInstance(string serviceTypeName)
        {
            var serviceType = Type.GetType(serviceTypeName, true);
            return Activator.CreateInstance(serviceType) as IWorkItem;
        }

        internal virtual bool SetupWorkItemInstance(IWorkItem workItem, WorkItemFactoryInfo factoryInfo)
        {
            try
            {
                //Share AppDomain AppServers also share same socket server factory and log factory instances
                factoryInfo.SocketServerFactory.ExportFactory.EnsureInstance();
                factoryInfo.LogFactory.ExportFactory.EnsureInstance();
            }
            catch (Exception e)
            {
                if (m_GlobalLog.IsErrorEnabled)
                    m_GlobalLog.Error(e);

                return false;
            }

            return workItem.Setup(this, factoryInfo.Config, factoryInfo.ProviderFactories.ToArray());
        }

        /// <summary>
        /// Gets the work item factory info loader.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        internal virtual WorkItemFactoryInfoLoader GetWorkItemFactoryInfoLoader(IConfigurationSource config, ILogFactory logFactory)
        {
            return new WorkItemFactoryInfoLoader(config, logFactory);
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration, config resolver and log factory.
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        public virtual bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver, ILogFactory logFactory)
        {
            if (m_Initialized)
                throw new Exception("The server had been initialized already, you cannot initialize it again!");

            if (logFactory != null && !string.IsNullOrEmpty(m_Config.LogFactory))
            {
                throw new ArgumentException("You cannot pass in a logFactory parameter, if you have configured a root log factory.", "logFactory");
            }

            IEnumerable<WorkItemFactoryInfo> workItemFactories;

            using (var factoryInfoLoader = GetWorkItemFactoryInfoLoader(m_Config, logFactory))
            {
                var bootstrapLogFactory = factoryInfoLoader.GetBootstrapLogFactory();

                logFactory = bootstrapLogFactory.ExportFactory.CreateExport<ILogFactory>();

                LogFactory = logFactory;
                m_GlobalLog = logFactory.GetLog(this.GetType().Name);

                try
                {
                    workItemFactories = factoryInfoLoader.LoadResult(serverConfigResolver);
                }
                catch (Exception e)
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error(e);

                    return false;
                }
            }

            m_AppServers = new List<IWorkItem>(m_Config.Servers.Count());
            //Initialize servers
            foreach (var factoryInfo in workItemFactories)
            {
                IWorkItem appServer;

                try
                {
                    appServer = CreateWorkItemInstance(factoryInfo.ServiceType);
                }
                catch (Exception e)
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error(string.Format("Failed to create server instance {0}!", factoryInfo.Config.Name), e);
                    return false;
                }


                var setupResult = false;

                try
                {
                    setupResult = SetupWorkItemInstance(appServer, factoryInfo);
                    setupResult = true;
                }
                catch (Exception e)
                {
                    m_GlobalLog.Error(e);
                    setupResult = false;
                }

                if (!setupResult)
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error("Failed to setup server instance!");
                    return false;
                }

                m_AppServers.Add(appServer);
            }

            if (!m_Config.DisablePerformanceDataCollector)
            {
                m_PerfMonitor = new PerformanceMonitor(m_Config, m_AppServers, logFactory);
                m_PerfMonitor.Collected += new EventHandler<PermformanceDataEventArgs>(m_PerfMonitor_Collected);
            }

            m_Initialized = true;

            return true;
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration and config resolver.
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <returns></returns>
        public virtual bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            return Initialize(serverConfigResolver, null);
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        public virtual bool Initialize(ILogFactory logFactory)
        {
            return Initialize(c => c, logFactory);
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <returns></returns>
        public virtual bool Initialize()
        {
            return Initialize(c => c);
        }

        /// <summary>
        /// Starts this bootstrap.
        /// </summary>
        /// <returns></returns>
        public StartResult Start()
        {
            if (!m_Initialized)
            {
                if (m_GlobalLog.IsErrorEnabled)
                    m_GlobalLog.Error("You cannot invoke method Start() before initializing!");

                return StartResult.Failed;
            }

            var result = StartResult.None;

            var succeeded = 0;

            foreach (var server in m_AppServers)
            {
                if (!server.Start())
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error("Failed to start " + server.Name + " server!");
                }
                else
                {
                    succeeded++;
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Info(server.Name + " has been started");
                }
            }

            if (m_AppServers.Any())
            {
                if (m_AppServers.Count == succeeded)
                    result = StartResult.Success;
                else if (m_AppServers.Count == 0)
                    result = StartResult.Failed;
                else
                    result = StartResult.PartialSuccess;
            }

            if (m_PerfMonitor != null)
                m_PerfMonitor.Start();

            return result;
        }

        /// <summary>
        /// Stops this bootstrap.
        /// </summary>
        public void Stop()
        {
            foreach (var server in m_AppServers)
            {
                if (server.IsRunning)
                {
                    server.Stop();

                    if (m_GlobalLog.IsInfoEnabled)
                        m_GlobalLog.Info(server.Name + " has been stopped");
                }
            }

            if (m_PerfMonitor != null)
                m_PerfMonitor.Stop();
        }

        void m_PerfMonitor_Collected(object sender, PermformanceDataEventArgs e)
        {
            var handler = PerformanceDataCollected;

            if (handler != null)
                handler(sender, e);
        }

        /// <summary>
        /// Occurs when [performance data collected].
        /// </summary>
        public event EventHandler<PermformanceDataEventArgs> PerformanceDataCollected;
    }
}
