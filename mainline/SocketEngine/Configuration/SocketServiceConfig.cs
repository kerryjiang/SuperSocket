using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Configuration;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// SuperSocket's root configuration node
    /// </summary>
    public class SocketServiceConfig : ConfigurationSection, IConfigurationSource
    {
        /// <summary>
        /// Gets all the server configurations
        /// </summary>
        [ConfigurationProperty("servers")]
        public ServerCollection Servers
        {
            get
            {
                return this["servers"] as ServerCollection;
            }
        }

        /// <summary>
        /// Gets the service configurations
        /// </summary>
        [ConfigurationProperty("services")]
        public TypeProviderCollection Services
        {
            get
            {
                return this["services"] as TypeProviderCollection;
            }
        }

        /// <summary>
        /// Gets all the connection filter configurations.
        /// </summary>
        [ConfigurationProperty("connectionFilters", IsRequired = false)]
        public TypeProviderCollection ConnectionFilters
        {
            get
            {
                return this["connectionFilters"] as TypeProviderCollection;
            }
        }

        /// <summary>
        /// Gets the defined log factory types.
        /// </summary>
        [ConfigurationProperty("logFactories", IsRequired = false)]
        public TypeProviderCollection LogFactories
        {
            get
            {
                return this["logFactories"] as TypeProviderCollection;
            }
        }

        /// <summary>
        /// Gets the logfactory name of the bootstrap.
        /// </summary>
        [ConfigurationProperty("requestFilterFactories", IsRequired = false)]
        public TypeProviderCollection RequestFilterFactories
        {
            get
            {
                return this["requestFilterFactories"] as TypeProviderCollection;
            }
        }

        /// <summary>
        /// Gets the command loaders definition.
        /// </summary>
        [ConfigurationProperty("commmandLoaders", IsRequired = false)]
        public TypeProviderCollection CommandLoaders
        {
            get
            {
                return this["commmandLoaders"] as TypeProviderCollection;
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

        /// <summary>
        /// Gets the isolation mode.
        /// </summary>
        [ConfigurationProperty("isolationMode", IsRequired = false, DefaultValue = IsolationMode.None)]
        public IsolationMode IsolationMode
        {
            get { return (IsolationMode)this["isolationMode"]; }
        }

        /// <summary>
        /// Gets the logfactory name of the bootstrap.
        /// </summary>
        [ConfigurationProperty("logFactory", IsRequired = false, DefaultValue = "")]
        public string LogFactory
        {
            get
            {
                return (string)this["logFactory"];
            }
        }

        IEnumerable<IServerConfig> IConfigurationSource.Servers
        {
            get
            {
                return this.Servers;
            }
        }

        IEnumerable<ITypeProvider> IConfigurationSource.Services
        {
            get
            {
                return this.Services;
            }
        }

        IEnumerable<ITypeProvider> IConfigurationSource.ConnectionFilters
        {
            get
            {
                return this.ConnectionFilters;
            }
        }

        IEnumerable<ITypeProvider> IConfigurationSource.LogFactories
        {
            get
            {
                return this.LogFactories;
            }
        }

        IEnumerable<ITypeProvider> IConfigurationSource.RequestFilterFactories
        {
            get
            {
                return this.RequestFilterFactories;
            }
        }


        IEnumerable<ITypeProvider> IConfigurationSource.CommandLoaders
        {
            get
            {
                return this.CommandLoaders;
            }
        }
    }
}
