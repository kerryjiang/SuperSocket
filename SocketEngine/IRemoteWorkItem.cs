using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// IRemoteWorkItem
    /// </summary>
    [ServiceContract]
    public interface IRemoteWorkItem
    {
        /// <summary>
        /// Setups the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        [OperationContract]
        bool Setup(IServerConfig config, ProviderFactoryInfo[] factories);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        [OperationContract]
        void Stop();

        /// <summary>
        /// Collects the server summary.
        /// </summary>
        /// <param name="nodeSummary">The node summary.</param>
        /// <returns></returns>
        [OperationContract]
        ServerSummary CollectServerSummary(NodeSummary nodeSummary);
    }
}
