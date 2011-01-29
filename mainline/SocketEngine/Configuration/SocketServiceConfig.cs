using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine.Configuration
{
    public class SocketServiceConfig : ConfigurationSection, IConfig
    {

        [ConfigurationProperty("servers")]
        public ServerCollection Servers
        {
            get
            {
                return this["servers"] as ServerCollection;
            }
        }

        [ConfigurationProperty("services")]
        public ServiceCollection Services
        {
            get
            {
                return this["services"] as ServiceCollection;
            }
        }

        [ConfigurationProperty("credential", IsRequired = false)]
        public CredentialConfig Credential
        {
            get
            {
                return this["credential"] as CredentialConfig;
            }
        }

        [ConfigurationProperty("consoleBaseAddress", IsRequired = false)]
        public string ConsoleBaseAddress
        {
            get
            {
                return this["consoleBaseAddress"] as string;
            }
        }

        [ConfigurationProperty("loggingMode", IsRequired = false, DefaultValue = "ShareFile")]
        public LoggingMode LoggingMode
        {
            get
            {
                return (LoggingMode)this["loggingMode"];
            }
        }

        [ConfigurationProperty("maxWorkingThreads", IsRequired = false, DefaultValue = -1)]
        public int MaxWorkingThreads
        {
            get
            {
                return (int)this["maxWorkingThreads"];
            }
        }

        [ConfigurationProperty("minWorkingThreads", IsRequired = false, DefaultValue = -1)]
        public int MinWorkingThreads
        {
            get
            {
                return (int)this["minWorkingThreads"];
            }
        }

        [ConfigurationProperty("maxCompletionPortThreads", IsRequired = false, DefaultValue = -1)]
        public int MaxCompletionPortThreads
        {
            get
            {
                return (int)this["maxCompletionPortThreads"];
            }
        }

        [ConfigurationProperty("minCompletionPortThreads", IsRequired = false, DefaultValue = -1)]
        public int MinCompletionPortThreads
        {
            get
            {
                return (int)this["minCompletionPortThreads"];
            }
        }

        #region IConfig Members

        public List<IServerConfig> GetServerList()
        {
            List<IServerConfig> serverList = new List<IServerConfig>();

            foreach (Server server in Servers)
            {
                serverList.Add(server);
            }

            return serverList;
        }

        public List<IServiceConfig> GetServiceList()
        {
            List<IServiceConfig> serviceList = new List<IServiceConfig>();

            foreach (Service service in Services)
            {
                serviceList.Add(service);
            }

            return serviceList;
        }

        public ICredentialConfig CredentialConfig
        {
            get { return Credential; }
        }

        #endregion
    }
}
