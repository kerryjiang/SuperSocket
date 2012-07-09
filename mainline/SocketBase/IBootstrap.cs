using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common.Logging;
using System.Configuration;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The bookstrap start result
    /// </summary>
    public enum StartResult
    {
        /// <summary>
        /// No appserver has been set in the bootstrap, so nothing was started
        /// </summary>
        None,
        /// <summary>
        /// All appserver instances were started successfully
        /// </summary>
        Success,
        /// <summary>
        /// Some appserver instances were started successfully, but some of them failed
        /// </summary>
        PartialSuccess,
        /// <summary>
        /// All appserver instances failed to start
        /// </summary>
        Failed
    }

    /// <summary>
    /// SuperSocket bootstrap
    /// </summary>
    public interface IBootstrap
    {
        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        IEnumerable<IWorkItem> AppServers { get; }

        /// <summary>
        /// Gets the config.
        /// </summary>
        IRootConfig Config { get; }

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <returns></returns>
        bool Initialize();


        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <returns></returns>
        bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver);


        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        bool Initialize(ILogFactory logFactory);

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver, ILogFactory logFactory);

        /// <summary>
        /// Starts this bootstrap.
        /// </summary>
        /// <returns></returns>
        StartResult Start();

        /// <summary>
        /// Stops this bootstrap.
        /// </summary>
        void Stop();

        /// <summary>
        /// Occurs when [performance data collected].
        /// </summary>
        event EventHandler<PermformanceDataEventArgs> PerformanceDataCollected;
    }
}
