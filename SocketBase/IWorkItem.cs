using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItemBase : IStatusInfoSource, ISystemEndPoint
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>return true if start successfull, else false</returns>
        bool Start();

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        int SessionCount { get; }
    }


    /// <summary>
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItem : IWorkItemBase, IStatusInfoSource
    {
        /// <summary>
        /// Setups with the specified root config.
        /// </summary>
        /// <param name="bootstrap">The bootstrap.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories);

        /// <summary>
        /// Gets the current state of the work item.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        ServerState State { get; }
    }
}
