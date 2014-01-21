using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System.Collections.Specialized;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// SuperSocket's root configuration node
    /// </summary>
    public partial class SocketServiceConfig : ConfigurationSection, IConfigurationSource
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
        [ConfigurationProperty("serverTypes")]
        public TypeProviderCollection ServerTypes
        {
            get
            {
                return this["serverTypes"] as TypeProviderCollection;
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
        [ConfigurationProperty("receiveFilterFactories", IsRequired = false)]
        public TypeProviderCollection ReceiveFilterFactories
        {
            get
            {
                return this["receiveFilterFactories"] as TypeProviderCollection;
            }
        }

        /// <summary>
        /// Gets the command loaders definition.
        /// </summary>
        [ConfigurationProperty("commandLoaders", IsRequired = false)]
        public TypeProviderCollection CommandLoaders
        {
            get
            {
                return this["commandLoaders"] as TypeProviderCollection;
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
        [ConfigurationProperty("isolation", IsRequired = false, DefaultValue = IsolationMode.None)]
        public IsolationMode Isolation
        {
            get { return (IsolationMode)this["isolation"]; }
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

        /// <summary>
        /// Gets the option elements.
        /// </summary>
        public NameValueCollection OptionElements { get; private set; }

        /// <summary>
        /// Gets a value indicating whether an unknown element is encountered during deserialization.
        /// To keep compatible with old configuration
        /// </summary>
        /// <param name="elementName">The name of the unknown subelement.</param>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> being used for deserialization.</param>
        /// <returns>
        /// true when an unknown element is encountered while deserializing; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element identified by <paramref name="elementName"/> is locked.- or -One or more of the element's attributes is locked.- or -<paramref name="elementName"/> is unrecognized, or the element has an unrecognized attribute.- or -The element has a Boolean attribute with an invalid value.- or -An attempt was made to deserialize a property more than once.- or -An attempt was made to deserialize a property that is not a valid member of the element.- or -The element cannot contain a CDATA or text element.</exception>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            //To keep compatible with old configuration
            if (!"services".Equals(elementName, StringComparison.OrdinalIgnoreCase))
            {
                if (OptionElements == null)
                    OptionElements = new NameValueCollection();

                OptionElements.Add(elementName, reader.ReadOuterXml());
                return true;
            }

            var serverTypes = new TypeProviderCollection();
            reader.Read();
            serverTypes.Deserialize(reader);

            this["serverTypes"] = serverTypes;

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            const string xmlns = "xmlns";
            const string xmlnsPrefix = "xmlns:";
            const string xsiPrefix = "xsi:";

            //for configuration intellisense, allow these unrecognized attributes: xmlns, xmlns:*, xsi:*
            if (name.Equals(xmlns) || name.StartsWith(xmlnsPrefix) || name.StartsWith(xsiPrefix))
                return true;

            return false;
        }

        /// <summary>
        /// Gets the child config.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config.</typeparam>
        /// <param name="childConfigName">Name of the child config.</param>
        /// <returns></returns>
        public TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }

        IEnumerable<IServerConfig> IConfigurationSource.Servers
        {
            get
            {
                return this.Servers;
            }
        }

        IEnumerable<ITypeProvider> IConfigurationSource.ServerTypes
        {
            get
            {
                return this.ServerTypes;
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

        IEnumerable<ITypeProvider> IConfigurationSource.ReceiveFilterFactories
        {
            get
            {
                return this.ReceiveFilterFactories;
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
