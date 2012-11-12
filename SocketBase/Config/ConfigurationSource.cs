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
            : base(source)
        {
            if (source.Servers != null && source.Servers.Any())
            {
                this.Servers = source.Servers.Select(s => new ServerConfig(s)).ToArray();
            }

            if (source.ServerTypes != null && source.ServerTypes.Any())
            {
                this.ServerTypes = source.ServerTypes.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }

            if (source.ConnectionFilters != null && source.ConnectionFilters.Any())
            {
                this.ConnectionFilters = source.ConnectionFilters.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }

            if (source.LogFactories != null && source.LogFactories.Any())
            {
                this.LogFactories = source.LogFactories.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
            }

            if (source.ReceiveFilterFactories != null && source.ReceiveFilterFactories.Any())
            {
                this.ReceiveFilterFactories = source.ReceiveFilterFactories.Select(s => s.CopyPropertiesTo(new TypeProviderConfig())).ToArray();
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
        /// Gets/sets the server types definition.
        /// </summary>
        public IEnumerable<ITypeProvider> ServerTypes { get; set; }

        /// <summary>
        /// Gets/sets the connection filters definition.
        /// </summary>
        public IEnumerable<ITypeProvider> ConnectionFilters { get; set; }

        /// <summary>
        /// Gets/sets the log factories definition.
        /// </summary>
        public IEnumerable<ITypeProvider> LogFactories { get; set; }

        /// <summary>
        /// Gets/sets the Receive filter factories definition.
        /// </summary>
        public IEnumerable<ITypeProvider> ReceiveFilterFactories { get; set; }

        /// <summary>
        /// Gets/sets the command loaders definition.
        /// </summary>
        public IEnumerable<ITypeProvider> CommandLoaders { get; set; }
    }
}
