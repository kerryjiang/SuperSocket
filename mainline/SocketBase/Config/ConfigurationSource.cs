using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Poco configuration source
    /// </summary>
    [Serializable]
    public class ConfigurationSource : RootConfig, IConfigurationSource
    {
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
