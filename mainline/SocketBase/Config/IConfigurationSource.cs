using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Configuration source interface
    /// </summary>
    public interface IConfigurationSource : IRootConfig
    {
        /// <summary>
        /// Gets the servers definitions.
        /// </summary>
        IEnumerable<IServerConfig> Servers { get; }

        /// <summary>
        /// Gets the services definition.
        /// </summary>
        IEnumerable<ITypeProvider> Services { get; }

        /// <summary>
        /// Gets the connection filters definition.
        /// </summary>
        IEnumerable<ITypeProvider> ConnectionFilters { get; }

        /// <summary>
        /// Gets the log factories definition.
        /// </summary>
        IEnumerable<ITypeProvider> LogFactories { get; }

        /// <summary>
        /// Gets the request filter factories definition.
        /// </summary>
        IEnumerable<ITypeProvider> RequestFilterFactories { get; }

        /// <summary>
        /// Gets the command loaders definition.
        /// </summary>
        IEnumerable<ITypeProvider> CommandLoaders { get; }
    }
}
