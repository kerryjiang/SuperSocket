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
        /// Gets the appServer types definition.
        /// </summary>
        IEnumerable<ITypeProvider> ServerTypes { get; }

        /// <summary>
        /// Gets the connection filters definition.
        /// </summary>
        IEnumerable<ITypeProvider> ConnectionFilters { get; }

        /// <summary>
        /// Gets the log factories definition.
        /// </summary>
        IEnumerable<ITypeProvider> LogFactories { get; }

        /// <summary>
        /// Gets the Receive filter factories definition.
        /// </summary>
        IEnumerable<ITypeProvider> ReceiveFilterFactories { get; }

        /// <summary>
        /// Gets the command loaders definition.
        /// </summary>
        IEnumerable<ITypeProvider> CommandLoaders { get; }
    }
}
