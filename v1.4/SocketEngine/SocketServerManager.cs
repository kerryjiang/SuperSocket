using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    public static partial class SocketServerManager
    {
        /// <summary>
        /// main AppServer instances list
        /// </summary>
        private static List<IAppServer> m_ServerList;

        private static Dictionary<string, Type> m_ServiceDict;
        
        private static Dictionary<string, IConnectionFilter> m_ConnectionFilterDict;

        private static IConfig m_Config;

        /// <summary>
        /// Indicate whether the server has been initialized
        /// </summary>
        private static bool m_Initialized = false;

        static SocketServerManager()
        {
            Platform.Initialize();
        }

        /// <summary>
        /// Initializes SuperSocket with the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <returns></returns>
        public static bool Initialize(IConfig config, Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            if (m_Initialized)
                throw new Exception("The server had been initialized already, you cannot initialize it again!");

            m_Config = config;            

            //Initialize services
            m_ServiceDict = new Dictionary<string, Type>(config.Services.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (var service in config.Services)
            {
                if (service.Disabled)
                    continue;
                
                Type serviceType;
                Exception exception;
                if (!AssemblyUtil.TryGetType(service.Type, out serviceType, out exception))
                {
                    LogUtil.LogError("Failed to initialize service " + service.Name + "!", exception);
                    return false;
                }

                m_ServiceDict[service.Name] = serviceType;
            }

            //Initialize connection filters
            m_ConnectionFilterDict = new Dictionary<string, IConnectionFilter>(config.ConnectionFilters.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (var filter in config.ConnectionFilters)
            {                
                Type filterType;
                Exception exception;
                if (!AssemblyUtil.TryGetType(filter.Type, out filterType, out exception))
                {
                    LogUtil.LogError("Failed to initialize connection filter " + filter.Name + "!", exception);
                    return false;
                }
                
                IConnectionFilter filterInstance;
                
                try
                {
                    filterInstance = (IConnectionFilter)Activator.CreateInstance(filterType);

                    if (!filterInstance.Initialize(filter.Name, filter.Options))
                        return false;
                }
                catch (Exception e)
                {
                    LogUtil.LogError(string.Format("Failed to initialize filter instance {0}!", filter.Name), e);
                    return false;
                }
                
                m_ConnectionFilterDict[filter.Name] = filterInstance;
            }            

            m_ServerList = new List<IAppServer>(config.Servers.Count());
            //Initialize servers
            foreach (var serverConfig in config.Servers)
            {
                if (string.IsNullOrEmpty(serverConfig.Name))
                {
                    LogUtil.LogError("The name attribute of server node is required!");
                    return false;
                }

                if (!InitializeServer(serverConfigResolver(serverConfig)))
                {
                    LogUtil.LogError("Failed to initialize server " + serverConfig.Name + "!");
                    return false;
                }
            }

            m_Initialized = true;

            return true;
        }

        /// <summary>
        /// Initializes SuperSocket with the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public static bool Initialize(IConfig config)
        {
            return Initialize(config, c => c);
        }

        /// <summary>
        /// Initializes the specified servers.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="servers">The passed in AppServers, which have been setup.</param>
        /// <returns></returns>
        public static bool Initialize(IRootConfig rootConfig, IEnumerable<IAppServer> servers)
        {
            if (rootConfig == null)
                throw new ArgumentNullException("rootConfig");

            var config = new ConfigMockup();
            rootConfig.CopyPropertiesTo(config);

            m_ServerList = new List<IAppServer>(servers.Count());
            m_ServerList.AddRange(servers);
            m_Initialized = true;

            m_Config = config;

            return true;
        }

        /// <summary>
        /// Initializes the specified servers.
        /// </summary>
        /// <param name="servers">The passed in AppServers, which have been setup.</param>
        /// <returns></returns>
        public static bool Initialize(IEnumerable<IAppServer> servers)
        {
            return Initialize(new RootConfig(), servers);
        }

        private static bool InitializeServer(IServerConfig serverConfig)
        {
            if (serverConfig.Disabled)
                return true;

            Type serviceType = null;

            if (!m_ServiceDict.TryGetValue(serverConfig.ServiceName, out serviceType))
            {
                LogUtil.LogError(string.Format("The service {0} cannot be found in configuration!", serverConfig.ServiceName));
                return false;
            }

            IAppServer server;

            try
            {
                server = (IAppServer)Activator.CreateInstance(serviceType);
            }
            catch (Exception e)
            {
                LogUtil.LogError("Failed to create server instance!", e);
                return false;
            }

            if (!server.Setup(m_Config, serverConfig, SocketServerFactory.Instance))
            {
                LogUtil.LogError("Failed to setup server instance!");
                return false;
            }
            
            if(!string.IsNullOrEmpty(serverConfig.ConnectionFilters))
            {
                var filters = serverConfig.ConnectionFilters.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                if(filters != null && filters.Length > 0)
                {
                    var filterInstances = new List<IConnectionFilter>(filters.Length);
                    
                    foreach(var f in filters)
                    {
                        IConnectionFilter currentFilter;
                        if(!m_ConnectionFilterDict.TryGetValue(f, out currentFilter))
                        {
                            LogUtil.LogError(string.Format("Failed to find a connection filter '{0}'!", f)); 
                            return false;
                        }
                        filterInstances.Add(currentFilter);
                    }
                    
                    server.ConnectionFilters = filterInstances;
                }
            }

            m_ServerList.Add(server);
            return true;
        }

        public static bool Start()
        {
            if (!m_Initialized)
            {
                LogUtil.LogError("You cannot invoke method Start() before initializing!");
                return false;
            }

            foreach (IAppServer server in m_ServerList)
            {
                if (!server.Start())
                {
                    LogUtil.LogError("Failed to start " + server.Name + " server!");
                }
                else
                {
                    LogUtil.LogInfo(server.Name + " has been started");
                }
            }

            Messanger.Send<List<IAppServer>>(m_ServerList);

            if(!m_Config.DisablePerformanceDataCollector)
                StartPerformanceLog();

            return true;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public static void Stop()
        {
            foreach (var server in m_ServerList)
            {
                server.Stop();
                LogUtil.LogInfo(server.Name + " has been stopped");
            }

            StopPerformanceLog();
        }

        public static IServiceConfig GetServiceConfig(string name)
        {
            foreach (var config in m_Config.Services)
            {
                if (string.Compare(config.Name, name, true) == 0)
                {
                    return config;
                }
            }
            return null;
        }


        public static IAppServer GetServerByName(string name)
        {
            return m_ServerList.SingleOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        class ConfigMockup : RootConfig, IConfig
        {
            private List<IServerConfig> m_Servers = new List<IServerConfig>(0);

            public IEnumerable<IServerConfig> Servers
            {
                get { return m_Servers; }
            }

            private List<IServiceConfig> m_Services = new List<IServiceConfig>(0);

            public IEnumerable<IServiceConfig> Services
            {
                get { return m_Services; }
            }

            private List<IConnectionFilterConfig> m_ConnectionFilters = new List<IConnectionFilterConfig>(0);

            public IEnumerable<IConnectionFilterConfig> ConnectionFilters
            {
                get { return m_ConnectionFilters; }
            }
        }
    }
}
