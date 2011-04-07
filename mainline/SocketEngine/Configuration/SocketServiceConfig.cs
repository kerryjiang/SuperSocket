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

        [ConfigurationProperty("genericServers", IsRequired = false)]
        public GenericServerConfigCollection GenericServers
        {
            get
            {
                return this["genericServers"] as GenericServerConfigCollection;
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
        
        [ConfigurationProperty("connectionFilters", IsRequired = false)]
        public ConnectionFilterConfigCollection ConnectionFilters
        {
            get
            {
                return this["connectionFilters"] as ConnectionFilterConfigCollection;
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
        
        #region IConfig implementation
        
        IEnumerable<IServerConfig> IConfig.Servers
        {
            get
            {
                return this.Servers;
            }
        }

        IEnumerable<IServiceConfig> IConfig.Services
        {
            get
            {
                return this.Services;
            }
        }
        
        IEnumerable<IConnectionFilterConfig> IConfig.ConnectionFilters
        {
            get
            {
                return this.ConnectionFilters;
            }
        }

        IEnumerable<IGenericServerConfig> IConfig.GenericServers
        {
            get
            {
                return this.GenericServers;
            }
        }
        
        ICredentialConfig IRootConfig.CredentialConfig
        {
            get { return this.Credential; }
        }
        
        #endregion
    }
}
