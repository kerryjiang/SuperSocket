using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The interface of server container
    /// </summary>
    public interface IServerContainer
    {
        /// <summary>
        /// Gets all application servers.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAppServer> GetAllServers();

        /// <summary>
        /// Gets the specific app server instance by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IAppServer GetServerByName(string name);

        /// <summary>
        /// Occurs when [performance data collect event is fired].
        /// </summary>
        event EventHandler<PermformanceDataEventArgs> PerformanceDataCollected;

        /// <summary>
        /// Occurs when [loaded].
        /// </summary>
        event EventHandler Loaded;
    }
}
