using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// SuperSocket default bootstrap
    /// </summary>
    public class DefaultBootstrap : IBootstrap
    {
        private List<IAppServer> m_AppServers;

        /// <summary>
        /// Indicates whether the bootstrap is initialized
        /// </summary>
        private bool m_Initialized = false;

        /// <summary>
        /// Service types dictionary which have been loaded
        /// </summary>
        private Dictionary<string, Type> m_ServiceDict;

        /// <summary>
        /// Connection filter types which have been loaded
        /// </summary>
        private Dictionary<string, Type> m_ConnectionFilterDict;

        /// <summary>
        /// Global configuration
        /// </summary>
        private IRootConfig m_Config;

        /// <summary>
        /// Global log
        /// </summary>
        private ILog m_GlobalLog;

        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        public IEnumerable<IAppServer> AppServers
        {
            get { return m_AppServers; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        public DefaultBootstrap()
        {

        }

        /// <summary>
        /// Initializes the bootstrap with the configuration, config resolver and log factory.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        public virtual bool Initialize(IConfig config, Func<IServerConfig, IServerConfig> serverConfigResolver, ILogFactory logFactory)
        {
            if (m_Initialized)
                throw new Exception("The server had been initialized already, you cannot initialize it again!");

            m_Config = config;

            if (LogFactoryProvider.LogFactory == null)
            {
                if (logFactory == null)
                    throw new ArgumentNullException("logFactory");

                LogFactoryProvider.Initialize(logFactory);
            }

            m_GlobalLog = LogFactoryProvider.GlobalLog;

            //Initialize services
            m_ServiceDict = new Dictionary<string, Type>(config.Services.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (var service in config.Services)
            {
                if (service.Disabled)
                    continue;

                Type serviceType;

                try
                {
                    serviceType = Type.GetType(service.Type, true);

                    if (serviceType == null)
                        throw new Exception(string.Format("Failed to get type {0}.", service.Type));
                }
                catch (Exception e)
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error("Failed to initialize service " + service.Name + "!", e);
                    return false;
                }

                m_ServiceDict[service.Name] = serviceType;
            }

            //Initialize connection filters
            m_ConnectionFilterDict = new Dictionary<string, Type>(config.ConnectionFilters.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (var filter in config.ConnectionFilters)
            {
                Type filterType;

                try
                {
                    filterType = Type.GetType(filter.Type, true);

                    if (filterType == null)
                        throw new NullReferenceException();
                }
                catch (Exception e)
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error("Failed to get filter type " + filter.Name + "!", e);
                    return false;
                }

                m_ConnectionFilterDict[filter.Name] = filterType;
            }

            m_AppServers = new List<IAppServer>(config.Servers.Count());
            //Initialize servers
            foreach (var serverConfig in config.Servers.OrderBy(s => s.StartupOrder))
            {
                if (string.IsNullOrEmpty(serverConfig.Name))
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error("The name attribute of server node is required!");
                    return false;
                }

                var appServer = InitializeServer(serverConfigResolver(serverConfig));

                if (appServer == null)
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error("Failed to initialize server " + serverConfig.Name + "!");
                    return false;
                }

                m_AppServers.Add(appServer);
            }

            m_Initialized = true;

            return true;
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration and config resolver.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <returns></returns>
        public virtual bool Initialize(IConfig config, Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            return Initialize(config, serverConfigResolver, new Log4NetLogFactory());
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public virtual bool Initialize(IConfig config)
        {
            return Initialize(config, c => c);
        }

        /// <summary>
        /// Initializes the bootstrap with initialized appserver instances.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="servers">The servers.</param>
        /// <param name="serverConfigs">The server configs.</param>
        /// <returns></returns>
        public virtual bool Initialize(IRootConfig rootConfig, IEnumerable<IAppServer> servers, IEnumerable<IServerConfig> serverConfigs)
        {
            return Initialize(rootConfig, servers, serverConfigs, new Log4NetLogFactory());
        }

        /// <summary>
        /// Initializes the bootstrap with initialized appserver instances.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="servers">The servers.</param>
        /// <param name="serverConfigs">The server configs.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        public virtual bool Initialize(IRootConfig rootConfig, IEnumerable<IAppServer> servers, IEnumerable<IServerConfig> serverConfigs, ILogFactory logFactory)
        {
            if (rootConfig == null)
                throw new ArgumentNullException("rootConfig");

            if (servers == null)
                throw new ArgumentNullException("servers");

            if (serverConfigs == null)
                throw new ArgumentNullException("serverConfigs");

            if (logFactory == null)
                throw new ArgumentNullException("logFactory");

            if (LogFactoryProvider.LogFactory == null)
            {
                if (logFactory == null)
                    throw new ArgumentNullException("logFactory");

                LogFactoryProvider.Initialize(logFactory);
            }

            m_Config = rootConfig;
            m_GlobalLog = LogFactoryProvider.GlobalLog;

            if (!servers.Any())
            {
                m_GlobalLog.Error("There must be one item in servers at least!");
                return false;
            }

            if (!serverConfigs.Any())
            {
                m_GlobalLog.Error("There must be one item in serverConfigs at least!");
                return false;
            }

            if (servers.Count() != serverConfigs.Count())
            {
                m_GlobalLog.Error("There must be one item in serverConfigs at least!");
                return false;
            }

            m_AppServers = new List<IAppServer>(servers.Count());

            var config = serverConfigs.GetEnumerator();

            foreach (var s in servers)
            {
                config.MoveNext();

                if (!s.Setup(this, rootConfig, config.Current, SocketServerFactory.Instance))
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error("Failed to setup server instance!");

                    return false;
                }

                m_AppServers.Add(s);
            }

            m_Initialized = true;
            return true;
        }

        /// <summary>
        /// Creates the connection filter.
        /// </summary>
        /// <param name="filterName">Name of the filter.</param>
        /// <returns></returns>
        private IConnectionFilter CreateConnectionFilter(string filterName)
        {
            Type filterType;

            if (!m_ConnectionFilterDict.TryGetValue(filterName, out filterType))
                return null;

            IConnectionFilter filter;

            try
            {
                filter = (IConnectionFilter)Activator.CreateInstance(filterType);
            }
            catch (Exception e)
            {
                if (m_GlobalLog.IsErrorEnabled)
                    m_GlobalLog.Error("Failed to create connection filter instance!", e);
                return null;
            }

            return filter;
        }

        /// <summary>
        /// Initializes the server with the server's configuration.
        /// </summary>
        /// <param name="serverConfig">The server config.</param>
        /// <returns></returns>
        private IAppServer InitializeServer(IServerConfig serverConfig)
        {
            if (serverConfig.Disabled)
                return null;

            Type serviceType = null;

            if (!m_ServiceDict.TryGetValue(serverConfig.ServiceName, out serviceType))
            {
                if (m_GlobalLog.IsErrorEnabled)
                    m_GlobalLog.ErrorFormat("The service {0} cannot be found in configuration!", serverConfig.ServiceName);
                return null;
            }

            IAppServer server;

            try
            {
                server = (IAppServer)Activator.CreateInstance(serviceType);
            }
            catch (Exception e)
            {
                if (m_GlobalLog.IsErrorEnabled)
                    m_GlobalLog.Error("Failed to create server instance!", e);
                return null;
            }

            if (!server.Setup(this, m_Config, serverConfig, SocketServerFactory.Instance))
            {
                if (m_GlobalLog.IsErrorEnabled)
                    m_GlobalLog.Error("Failed to setup server instance!");
                return null;
            }

            if (!string.IsNullOrEmpty(serverConfig.ConnectionFilters))
            {
                var filters = serverConfig.ConnectionFilters.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (filters != null && filters.Length > 0)
                {
                    var filterInstances = new List<IConnectionFilter>(filters.Length);

                    foreach (var f in filters)
                    {
                        IConnectionFilter currentFilter = CreateConnectionFilter(f);

                        if (currentFilter == null)
                        {
                            if (m_GlobalLog.IsErrorEnabled)
                                m_GlobalLog.ErrorFormat("Failed to find or create a connection filter '{0}'!", f);
                            return null;
                        }

                        if (!currentFilter.Initialize(f, server))
                        {
                            if (m_GlobalLog.IsErrorEnabled)
                                m_GlobalLog.ErrorFormat("Failed to initialize a connection filter '{0}'!", f);
                            return null;
                        }

                        filterInstances.Add(currentFilter);
                    }

                    server.ConnectionFilters = filterInstances;
                }
            }

            return server;
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

            foreach (IAppServer server in m_AppServers)
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

            if (!m_Config.DisablePerformanceDataCollector)
                StartPerformanceLog();

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

            if (!m_Config.DisablePerformanceDataCollector)
                StopPerformanceLog();
        }

        private Timer m_PerformanceTimer;
        private int m_TimerInterval;
        private ILog m_PerfLog;

        private PerformanceCounter m_CpuUsagePC;
        private PerformanceCounter m_ThreadCountPC;
        private PerformanceCounter m_WorkingSetPC;

        private int m_CpuCores = 1;

        private EventHandler<PermformanceDataEventArgs> m_PerformanceDataCollected;

        /// <summary>
        /// Occurs when [performance data collected].
        /// </summary>
        public event EventHandler<PermformanceDataEventArgs> PerformanceDataCollected
        {
            add { m_PerformanceDataCollected += value; }
            remove { m_PerformanceDataCollected -= value; }
        }

        private void StartPerformanceLog()
        {
            Process process = Process.GetCurrentProcess();

            m_CpuCores = Environment.ProcessorCount;

            m_CpuUsagePC = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
            m_ThreadCountPC = new PerformanceCounter("Process", "Thread Count", process.ProcessName);
            m_WorkingSetPC = new PerformanceCounter("Process", "Working Set", process.ProcessName);

            m_PerfLog = LogFactoryProvider.LogFactory.GetLog("performance");

            m_TimerInterval = m_Config.PerformanceDataCollectInterval * 1000;
            m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
            m_PerformanceTimer.Change(0, m_TimerInterval);
        }

        private void StopPerformanceLog()
        {
            m_PerformanceTimer.Change(Timeout.Infinite, Timeout.Infinite);

            m_CpuUsagePC.Close();
            m_ThreadCountPC.Close();
            m_WorkingSetPC.Close();
        }

        private void OnPerformanceTimerCallback(object state)
        {
            int availableWorkingThreads, availableCompletionPortThreads;
            ThreadPool.GetAvailableThreads(out availableWorkingThreads, out availableCompletionPortThreads);

            int maxWorkingThreads;
            int maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkingThreads, out maxCompletionPortThreads);

            var globalPerfData = new GlobalPerformanceData
            {
                AvailableWorkingThreads = availableWorkingThreads,
                AvailableCompletionPortThreads = availableCompletionPortThreads,
                MaxCompletionPortThreads = maxCompletionPortThreads,
                MaxWorkingThreads = maxWorkingThreads,
                CpuUsage = m_CpuUsagePC.NextValue() / m_CpuCores,
                TotalThreadCount = (int)m_ThreadCountPC.NextValue(),
                WorkingSet = (long)m_WorkingSetPC.NextValue()
            };

            var perfBuilder = new StringBuilder();

            perfBuilder.AppendLine("---------------------------------------------------");
            perfBuilder.AppendLine(string.Format("CPU Usage: {0}%, Physical Memory Usage: {1:N}, Total Thread Count: {2}", globalPerfData.CpuUsage.ToString("0.00"), globalPerfData.WorkingSet, globalPerfData.TotalThreadCount));
            perfBuilder.AppendLine(string.Format("AvailableWorkingThreads: {0}, AvailableCompletionPortThreads: {1}", globalPerfData.AvailableWorkingThreads, globalPerfData.AvailableCompletionPortThreads));
            perfBuilder.AppendLine(string.Format("MaxWorkingThreads: {0}, MaxCompletionPortThreads: {1}", globalPerfData.MaxWorkingThreads, globalPerfData.MaxCompletionPortThreads));

            var instancesData = new List<PerformanceDataInfo>(m_AppServers.Count);

            m_AppServers.ForEach(s =>
            {
                var perfSource = s as IPerformanceDataSource;
                if (perfSource != null)
                {
                    var perfData = perfSource.CollectPerformanceData(globalPerfData);

                    instancesData.Add(new PerformanceDataInfo { ServerName = s.Name, Data = perfData });

                    perfBuilder.AppendLine(string.Format("{0} - Total Connections: {1}, Total Handled Requests: {2}, Request Handling Speed: {3:f0}/s",
                        s.Name,
                        perfData.CurrentRecord.TotalConnections,
                        perfData.CurrentRecord.TotalHandledRequests,
                        (perfData.CurrentRecord.TotalHandledRequests - perfData.PreviousRecord.TotalHandledRequests) / perfData.CurrentRecord.RecordSpan));
                }
            });

            m_PerfLog.Info(perfBuilder.ToString());

            var handler = m_PerformanceDataCollected;
            if (handler == null)
                return;

            handler.BeginInvoke(this, new PermformanceDataEventArgs(globalPerfData, instancesData.ToArray()), null, null);
        }
    }
}
