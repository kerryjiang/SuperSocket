using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.Agent
{
    /// <summary>
    /// The service exposed to bootstrap to control the agent
    /// </summary>
    public class WorkItemAgent : IRemoteWorkItem
    {
        /// <summary>
        /// Setups the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Setup(IServerConfig config, ProviderFactoryInfo[] factories)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Start()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Stop()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Collects the server summary.
        /// </summary>
        /// <param name="nodeSummary">The node summary.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ServerSummary CollectServerSummary(NodeSummary nodeSummary)
        {
            throw new NotImplementedException();
        }
    }
}
