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

        [ConfigurationProperty("protocols", IsRequired = false)]
        public ProtocolCollectionConfig Protocols
        {
            get
            {
                return this["protocols"] as ProtocolCollectionConfig;
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

        [Obsolete]
        [ConfigurationProperty("independentLogger", IsRequired = false, DefaultValue = false)]
        public bool IndependentLogger
        {
            get
            {
                return (bool)this["independentLogger"];
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

        public List<IProtocolConfig> GetProtocolList()
        {
            var protocolList = new List<IProtocolConfig>();

            foreach (ProtocolConfig protocol in Protocols)
            {
                protocolList.Add(protocol);
            }

            return protocolList;
        }

        #endregion
    }
}
