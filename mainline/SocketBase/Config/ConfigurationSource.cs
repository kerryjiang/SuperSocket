using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Poco configuration source
    /// </summary>
    [Serializable]
    public class ConfigurationSource : RootConfig, IConfigurationSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSource"/> class.
        /// </summary>
        public ConfigurationSource()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ConfigurationSource(IConfigurationSource source)
        {
            source.CopyPropertiesTo(this);

            if (source.Servers != null && source.Servers.Any())
            {
                this.Servers = source.Servers.Select(s => new ServerConfig(s)).ToArray();
            }

            if (source.Services != null && source.Services.Any())
            {
                this.Services = source.Services.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }

            if (source.ConnectionFilters != null && source.ConnectionFilters.Any())
            {
                this.ConnectionFilters = source.ConnectionFilters.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }

            if (source.LogFactories != null && source.LogFactories.Any())
            {
                this.LogFactories = source.LogFactories.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }

            if (source.RequestFilterFactories != null && source.RequestFilterFactories.Any())
            {
                this.RequestFilterFactories = source.RequestFilterFactories.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }

            if (source.CommandLoaders != null && source.CommandLoaders.Any())
            {
                this.CommandLoaders = source.CommandLoaders.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }
        }


        /// <summary>
        /// Gets the servers definitions.
        /// </summary>
        public IEnumerable<IServerConfig> Servers { get; set; }

        /// <summary>
        /// Gets the services definition.
        /// </summary>
        public IEnumerable<ITypeProvider> Services { get; set; }

        /// <summary>
        /// Gets the connection filters definition.
        /// </summary>
        public IEnumerable<ITypeProvider> ConnectionFilters { get; set; }

        /// <summary>
        /// Gets the log factories definition.
        /// </summary>
        public IEnumerable<ITypeProvider> LogFactories { get; set; }

        /// <summary>
        /// Gets the request filter factories definition.
        /// </summary>
        public IEnumerable<ITypeProvider> RequestFilterFactories { get; set; }

        /// <summary>
        /// Gets/sets the command loaders definition.
        /// </summary>
        public IEnumerable<ITypeProvider> CommandLoaders { get; set; }
    }
}
