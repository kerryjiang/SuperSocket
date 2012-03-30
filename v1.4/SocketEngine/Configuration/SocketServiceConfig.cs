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
        
        [ConfigurationProperty("connectionFilters", IsRequired = false)]
        public ConnectionFilterConfigCollection ConnectionFilters
        {
            get
            {
                return this["connectionFilters"] as ConnectionFilterConfigCollection;
            }
        }

        /// <summary>
        /// Gets the logging mode.
        /// </summary>
        [ConfigurationProperty("loggingMode", IsRequired = false, DefaultValue = "ShareFile")]
        public LoggingMode LoggingMode
        {
            get
            {
                return (LoggingMode)this["loggingMode"];
            }
        }

        /// <summary>
        /// Gets the max working threads.
        /// </summary>
        [ConfigurationProperty("maxWorkingThreads", IsRequired = false, DefaultValue = -1)]
        public int MaxWorkingThreads
        {
            get
            {
                return (int)this["maxWorkingThreads"];
            }
        }

        /// <summary>
        /// Gets the min working threads.
        /// </summary>
        [ConfigurationProperty("minWorkingThreads", IsRequired = false, DefaultValue = -1)]
        public int MinWorkingThreads
        {
            get
            {
                return (int)this["minWorkingThreads"];
            }
        }

        /// <summary>
        /// Gets the max completion port threads.
        /// </summary>
        [ConfigurationProperty("maxCompletionPortThreads", IsRequired = false, DefaultValue = -1)]
        public int MaxCompletionPortThreads
        {
            get
            {
                return (int)this["maxCompletionPortThreads"];
            }
        }

        /// <summary>
        /// Gets the min completion port threads.
        /// </summary>
        [ConfigurationProperty("minCompletionPortThreads", IsRequired = false, DefaultValue = -1)]
        public int MinCompletionPortThreads
        {
            get
            {
                return (int)this["minCompletionPortThreads"];
            }
        }

        /// <summary>
        /// Gets the performance data collect interval, in seconds.
        /// </summary>
        [ConfigurationProperty("performanceDataCollectInterval", IsRequired = false, DefaultValue = 60)]
        public int PerformanceDataCollectInterval
        {
            get
            {
                return (int)this["performanceDataCollectInterval"];
            }
        }

        /// <summary>
        /// Gets a value indicating whether [disable performance data collector].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable performance data collector]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("disablePerformanceDataCollector", IsRequired = false, DefaultValue = false)]
        public bool DisablePerformanceDataCollector
        {
            get
            {
                return (bool)this["disablePerformanceDataCollector"];
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
        
        #endregion
    }
}
