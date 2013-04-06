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
    public class WorkItemAgent : MarshalByRefObject, IRemoteWorkItem
    {
        private IWorkItem m_AppServer;

        private AssemblyImport m_AssemblyImporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemAgent" /> class.
        /// </summary>
        public WorkItemAgent()
        {

        }

        public bool Setup(string serverType, string bootstrapUri, string assemblyImportRoot, IServerConfig config, ProviderFactoryInfo[] factories)
        {
            m_AssemblyImporter = new AssemblyImport(assemblyImportRoot);

            var serviceType = Type.GetType(serverType);
            m_AppServer = (IWorkItem)Activator.CreateInstance(serviceType);

            var bootstrap = (IBootstrap)Activator.GetObject(typeof(IBootstrap), bootstrapUri);

            return m_AppServer.Setup(bootstrap, config, factories);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Start()
        {
            return m_AppServer.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Stop()
        {
            m_AppServer.Stop();
        }

        /// <summary>
        /// Collects the server summary.
        /// </summary>
        /// <param name="nodeSummary">The node summary.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ServerSummary CollectServerSummary(NodeSummary nodeSummary)
        {
            return m_AppServer.CollectServerSummary(nodeSummary);
        }

        /// <summary>
        /// Gets the session count.
        /// </summary>
        /// <value>
        /// The session count.
        /// </value>
        public int SessionCount
        {
            get { return m_AppServer.SessionCount; }
        }
    }
}
