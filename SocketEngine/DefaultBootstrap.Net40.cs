using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine.Configuration;

namespace SuperSocket.SocketEngine
{
    public partial class DefaultBootstrap : IDynamicBootstrap
    {
        IWorkItem AddNewServer(IServerConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(config.Name))
                throw new ArgumentException("The new server's name cannot be empty.", "config");

            if (!m_Initialized)
                throw new Exception("The bootstrap must be initialized already!");

            if (m_AppServers.Any(s => config.Name.Equals(s.Name, StringComparison.OrdinalIgnoreCase)))
            {
                m_GlobalLog.ErrorFormat("The new server's name '{0}' has been taken by another server.", config.Name);
                return null;
            }

            var configSource = new ConfigurationSource(m_Config);
            configSource.Servers = new IServerConfig[] { config };

            IEnumerable<WorkItemFactoryInfo> workItemFactories;

            using (var factoryInfoLoader = GetWorkItemFactoryInfoLoader(configSource, LogFactory))
            {
                try
                {
                    workItemFactories = factoryInfoLoader.LoadResult((c) => c);
                }
                catch (Exception e)
                {
                    if (m_GlobalLog.IsErrorEnabled)
                        m_GlobalLog.Error(e);

                    return null;
                }
            }

            var server = InitializeAndSetupWorkItem(workItemFactories.FirstOrDefault());

            if (server != null)
            {
                m_AppServers.Add(server);

                if (!m_Config.DisablePerformanceDataCollector)
                {
                    ResetPerfMoniter();
                }

                var section = m_Config as SocketServiceConfig;

                if (section != null) //file configuration
                {
                    var serverConfig = new Server();
                    serverConfig.LoadFrom(config);
                    section.Servers.AddNew(serverConfig);
                    section.CurrentConfiguration.Save(ConfigurationSaveMode.Minimal);
                }
            }

            return server;
        }

        bool IDynamicBootstrap.Add(IServerConfig config)
        {
            var newWorkItem = AddNewServer(config);
            return newWorkItem != null;
        }

        bool IDynamicBootstrap.AddAndStart(IServerConfig config)
        {
            var newWorkItem = AddNewServer(config);

            if (newWorkItem == null)
                return false;

            return newWorkItem.Start();
        }

        void IDynamicBootstrap.Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var server = m_AppServers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (server == null)
                throw new Exception("The server is not found.");

            if (server.State != ServerState.NotStarted)
                throw new Exception("The server is running now, you cannot remove it. Please stop it at first.");

            m_AppServers.Remove(server);

            ResetPerfMoniter();

            var section = m_Config as SocketServiceConfig;

            if (section != null) //file configuration
            {
                section.Servers.Remove(name);
                section.CurrentConfiguration.Save(ConfigurationSaveMode.Minimal);
            }
        }
    }
}
