using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase.Config;
using System.Reflection;

namespace SuperSocket.SocketEngine
{
    class MarshalAppServer : MarshalByRefObject, IWorkItem
    {
        private IWorkItem m_AppServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainAppServer"/> class.
        /// </summary>
        /// <param name="serviceTypeName">Name of the service type.</param>
        public MarshalAppServer(string serviceTypeName)
        {
            var serviceType = Type.GetType(serviceTypeName);
            m_AppServer = (IWorkItem)Activator.CreateInstance(serviceType);
        }

        /// <summary>
        /// Gets the name of the server instance.
        /// </summary>
        public string Name
        {
            get { return m_AppServer.Name; }
        }

        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="bootstrap">The bootstrap.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="factories">The providers.</param>
        /// <returns></returns>
        public bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories)
        {
            return m_AppServer.Setup(bootstrap, config, factories);
        }

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>
        /// return true if start successfull, else false
        /// </returns>
        public bool Start()
        {
            return m_AppServer.Start();
        }

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        public void Stop()
        {
            m_AppServer.Stop();
        }

        /// <summary>
        /// Gets the current state of the work item.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public ServerState State
        {
            get { return m_AppServer.State; }
        }

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        public int SessionCount
        {
            get { return m_AppServer.SessionCount; }
        }


        public ServerSummary Summary
        {
            get { return m_AppServer.Summary; }
        }

        ServerSummary IWorkItem.CollectServerSummary(NodeSummary nodeSummary)
        {
            return m_AppServer.CollectServerSummary(nodeSummary);   
        }
    }
}
