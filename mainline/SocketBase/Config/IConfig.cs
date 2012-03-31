using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Server configuration interface
    /// </summary>
    public interface IConfig : IRootConfig
    {
        /// <summary>
        /// Gets all the server configurations
        /// </summary>
        IEnumerable<IServerConfig> Servers { get; }

        /// <summary>
        /// Gets the service configurations
        /// </summary>
        IEnumerable<IServiceConfig> Services { get; }

        /// <summary>
        /// Gets all the connection filter configurations.
        /// </summary>
        IEnumerable<IConnectionFilterConfig> ConnectionFilters { get; }
    }
}
