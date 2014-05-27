using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using System.Net;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The bootstrap start result
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
        /// Initializes the bootstrap with a listen endpoint replacement dictionary
        /// </summary>
        /// <param name="listenEndPointReplacement">The listen end point replacement.</param>
        /// <returns></returns>
        bool Initialize(IDictionary<string, IPEndPoint> listenEndPointReplacement);

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
        /// Gets the startup config file.
        /// </summary>
        string StartupConfigFile { get; }


        /// <summary>
        /// Gets the base directory.
        /// </summary>
        /// <value>
        /// The base directory.
        /// </value>
        string BaseDirectory { get; }
    }

    /// <summary>
    /// The bootstrap interface to support add new server instance in runtime
    /// </summary>
    public interface IDynamicBootstrap
    {
        /// <summary>
        /// Adds a new server into the bootstrap.
        /// </summary>
        /// <param name="config">The new server's config.</param>
        /// <returns></returns>
        bool Add(IServerConfig config);

        /// <summary>
        /// Adds a new server into the bootstrap and then start it.
        /// </summary>
        /// <param name="config">The new server's config.</param>
        /// <returns></returns>
        bool AddAndStart(IServerConfig config);


        /// <summary>
        /// Removes the server instance which is specified by name.
        /// </summary>
        /// <param name="name">The name of the server instance to be removed.</param>
        void Remove(string name);
    }
}
