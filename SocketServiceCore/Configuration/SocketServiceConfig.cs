using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore.Configuration
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

        [ConfigurationProperty("independentLogger", IsRequired = false, DefaultValue = false)]
        public bool IndependentLogger
        {
            get
            {
                return (bool)this["independentLogger"];
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
