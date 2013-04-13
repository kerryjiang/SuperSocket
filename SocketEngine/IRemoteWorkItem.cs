using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// IRemoteWorkItem
    /// </summary>
    public interface IRemoteWorkItem : IStatusInfoSource
    {
        /// <summary>
        /// Setups the specified config.
        /// </summary>
        /// <param name="serverType">Type of the server.</param>
        /// <param name="bootstrapUri">The bootstrap URI.</param>
        /// <param name="assemblyImportRoot">The assembly import root.</param>
        /// <param name="config">The config.</param>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        bool Setup(string serverType, string bootstrapUri, string assemblyImportRoot, IServerConfig config, ProviderFactoryInfo[] factories);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the session count.
        /// </summary>
        /// <value>
        /// The session count.
        /// </value>
        int SessionCount { get; }
    }
}
