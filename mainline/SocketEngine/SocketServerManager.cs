using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.ConnectionFilter;

namespace SuperSocket.SocketEngine
{
    public static partial class SocketServerManager
    {
        private static List<IAppServer> m_ServerList = new List<IAppServer>();

        private static Dictionary<string, Type> m_ServiceDict = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        
        private static Dictionary<string, IConnectionFilter> m_ConnectionFilterDict = new Dictionary<string, IConnectionFilter>(StringComparer.OrdinalIgnoreCase);

        private static IConfig m_Config;

        /// <summary>
        /// Initializes with the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public static bool Initialize(IConfig config)
        {
            m_Config = config;

            //Initialize services
            foreach (var service in config.Services)
            {
                if (service.Disabled)
                    continue;
                
                Type serviceType;

                if (!AssemblyUtil.TryGetType(service.Type, out serviceType))
                {
                    LogUtil.LogError("Failed to initialize service " + service.Name + "!");
                    return false;
                }

                m_ServiceDict[service.Name] = serviceType;
            }
            
            //Initialize connection filter
            foreach (var filter in config.ConnectionFilters)
            {
                
                Type filterType;

                if (!AssemblyUtil.TryGetType(filter.Type, out filterType))
                {
                    LogUtil.LogError("Failed to initialize connection filter " + filter.Name + "!");
                    return false;
                }
                
                IConnectionFilter filterInstance;
                
                try
                {
                    filterInstance = (IConnectionFilter)Activator.CreateInstance(filterType);
                    if(filterInstance == null)
                        throw new Exception(filterType.ToString());
                }
                catch (Exception e)
                {
                    LogUtil.LogError("Failed to create connection filter instance!", e);
                    return false;
                }
                
                if(!filterInstance.Initialize(filter.Name, filter.Options))
                {
                    LogUtil.LogError(string.Format("Failed to initialize filter instance {0}!", filter.Name));
                    return false;
                }
                
                m_ConnectionFilterDict[filter.Name] = filterInstance;
            }

            //Initialize servers
            foreach (var serverConfig in config.Servers)
            {
                if (!InitializeServer(serverConfig))
                {
                    LogUtil.LogError("Failed to initialize server " + serverConfig.Name + "!");
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes the specified servers.
        /// </summary>
        /// <param name="servers">The passed in AppServers, which have been setup.</param>
        /// <returns></returns>
        public static bool Initialize(IEnumerable<IAppServer> servers)
        {
            m_ServerList.AddRange(servers);
            return true;
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
    }
}
