using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketEngine.Configuration;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common.Logging;

namespace SuperSocket.SocketEngine
{
    class WorkItemFactoryInfoLoader
    {
        private ProviderFactoryInfo m_DefaultLogFactory;

        private IConfigurationSource m_Config;

        public WorkItemFactoryInfoLoader(IConfigurationSource config, ILogFactory passedInLogFactory)
            : this(config)
        {
            if (passedInLogFactory != null)
                m_DefaultLogFactory = new ProviderFactoryInfo(ProviderKey.LogFactory, string.Empty, passedInLogFactory);
        }

        public WorkItemFactoryInfoLoader(IConfigurationSource config)
        {
            m_Config = config;
        }

        public ProviderFactoryInfo GetBootstrapLogFactory()
        {
            if (m_DefaultLogFactory != null)
                return m_DefaultLogFactory;

            if (string.IsNullOrEmpty(m_Config.LogFactory))
            {
                m_DefaultLogFactory = new ProviderFactoryInfo(ProviderKey.LogFactory, string.Empty, typeof(Log4NetLogFactory));
                return m_DefaultLogFactory;
            }

            ProviderFactoryInfo factory = null;

            if (m_Config.LogFactories != null && m_Config.LogFactories.Count() > 0)
            {
                var logConfig = m_Config.LogFactories.FirstOrDefault(f =>
                    f.Name.Equals(m_Config.LogFactory, StringComparison.OrdinalIgnoreCase));

                if (logConfig != null)
                {
                    factory = new ProviderFactoryInfo(ProviderKey.LogFactory, m_Config.LogFactory, GetTypeByTypeProvider(ProviderKey.LogFactory, logConfig));
                }
            }

            if (factory == null)
                throw new Exception(string.Format("the specific log factory '{0}' cannot be found!", m_Config.LogFactory));

            m_DefaultLogFactory = factory;

            return factory;
        }

        public List<WorkItemFactoryInfo> LoadResult(Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            var workItemFactories = new List<WorkItemFactoryInfo>(m_Config.Servers.Count());

            //var providerFactories = new List<ProviderFactoryInfo>();

            //Initialize services
            var serviceFactories = InitializeProviderFactories(ProviderKey.Service, m_Config.Services);

            if (serviceFactories == null || !serviceFactories.Any())
                throw new Exception("Services configuration node is required!");

            //Initialize connection filters
            var connectionFilterFactories = InitializeProviderFactories(ProviderKey.ConnectionFilter, m_Config.ConnectionFilters);

            //Initialize log factories
            var logFactoryFactories = InitializeProviderFactories(ProviderKey.LogFactory, m_Config.LogFactories, m_DefaultLogFactory);

            //Initialize request filter factories
            var requestFilterFactories = InitializeProviderFactories(ProviderKey.RequestFilterFactory, m_Config.RequestFilterFactories);


            //Initialize command loader factories
            var commandLoaderFactories = InitializeProviderFactories(ProviderKey.CommandLoader, m_Config.CommandLoaders);

            //Initialize servers
            foreach (var c in m_Config.Servers.OrderBy(s => s.StartupOrder))
            {
                var serverConfig = serverConfigResolver(c);

                if (string.IsNullOrEmpty(serverConfig.Name))
                    throw new Exception("The name attribute of server node is required!");

                if (string.IsNullOrEmpty(serverConfig.ServiceName))
                    throw new Exception("The serviceName attribute of server node is required!");

                var serviceFactory = serviceFactories.FirstOrDefault(p => p.Name.Equals(serverConfig.ServiceName, StringComparison.OrdinalIgnoreCase));

                if (serviceFactory == null)
                    throw new Exception(string.Format("Failed to find a service for server {0}!", serverConfig.Name));

                var workItemFactory = new WorkItemFactoryInfo();
                workItemFactory.Config = serverConfig;
                workItemFactory.ServiceType = serviceFactory.ExportFactory.Type;

                var factories = new List<ProviderFactoryInfo>();

                workItemFactory.SocketServerFactory = new ProviderFactoryInfo(ProviderKey.SocketServerFactory, string.Empty, typeof(SocketServerFactory));

                factories.Add(workItemFactory.SocketServerFactory);

                //Initialize connection filters
                if(!string.IsNullOrEmpty(serverConfig.ConnectionFilter))
                {
                    var currentFactories = GetSelectedFactories(connectionFilterFactories, serverConfig.ConnectionFilter);

                    if (currentFactories.Any())
                        factories.AddRange(currentFactories);
                }

                //Initialize command loaders
                if (!string.IsNullOrEmpty(serverConfig.CommandLoader))
                {
                    var currentFactories = GetSelectedFactories(commandLoaderFactories, serverConfig.CommandLoader);

                    if (currentFactories.Any())
                        factories.AddRange(currentFactories);
                }

                var logFactoryName = ((Server)c).LogFactory;

                if (!string.IsNullOrEmpty(logFactoryName))
                {
                    logFactoryName = logFactoryName.Trim();

                    var logProviderFactory = logFactoryFactories.FirstOrDefault(p => p.Name.Equals(logFactoryName, StringComparison.OrdinalIgnoreCase));

                    if (logProviderFactory == null)
                        throw new Exception(string.Format("the specific log factory '{0}' cannot be found!", logFactoryName));

                    workItemFactory.LogFactory = logProviderFactory;
                }
                else
                {
                    workItemFactory.LogFactory = m_DefaultLogFactory;
                }

                factories.Add(workItemFactory.LogFactory);

                workItemFactory.ProviderFactories = factories;

                workItemFactories.Add(workItemFactory);
            }

            return workItemFactories;
        }

        private IEnumerable<ProviderFactoryInfo> GetSelectedFactories(List<ProviderFactoryInfo> source, string selectedItems)
        {
            var items = selectedItems.Split(new char[] { ',', ';' });

            if (items == null && !items.Any())
                return null;

            items = items.Select(f => f.Trim()).ToArray();

            return source.Where(p => items.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
        }

        private List<ProviderFactoryInfo> InitializeProviderFactories(ProviderKey key, IEnumerable<ITypeProvider> providerCollection)
        {
            return InitializeProviderFactories(key, providerCollection, null);
        }

        private List<ProviderFactoryInfo> InitializeProviderFactories(ProviderKey key, IEnumerable<ITypeProvider> providerCollection, ProviderFactoryInfo loadedFactory)
        {
            var loadedFactoryPassedIn = false;

            if(loadedFactory != null && !string.IsNullOrEmpty(loadedFactory.Name))
                loadedFactoryPassedIn = true;

            var factories = new List<ProviderFactoryInfo>();

            if (providerCollection == null || !providerCollection.Any())
            {
                return factories;
            }

            foreach (var provider in providerCollection)
            {
                if (loadedFactoryPassedIn)
                {
                    if (loadedFactory.Name.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        factories.Add(loadedFactory);
                        continue;
                    }
                }

                factories.Add(new ProviderFactoryInfo(key, provider.Name, GetTypeByTypeProvider(key, provider)));
            }

            return factories;
        }

        private Type GetTypeByTypeProvider(ProviderKey key, ITypeProvider provider)
        {
            try
            {
                var providerType = Type.GetType(provider.Type, true);

                if (providerType == null)
                    throw new NullReferenceException();

                return providerType;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to get {0} {1}'s type {2}.", key.Name, provider.Name, provider.Type), e);
            }
        }
    }
}
