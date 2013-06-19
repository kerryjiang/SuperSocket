using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase.Metadata;
using System.Runtime.Remoting.Lifetime;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.Agent
{
    /// <summary>
    /// The service exposed to bootstrap to control the agent
    /// </summary>
    public class WorkItemAgent : MarshalByRefObject, IRemoteWorkItem, IStatusInfoSource
    {
        private IWorkItem m_AppServer;

        private AssemblyImport m_AssemblyImporter;

        private ILog m_Log;

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

            var ret = m_AppServer.Setup(bootstrap, config, factories);

            if (ret)
            {
                m_Log = ((IAppServer)m_AppServer).Logger;
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            }

            return ret;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (m_Log != null)
            {
                m_Log.Error("The process crashed for an unhandled exception!", (Exception)e.ExceptionObject);
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            return m_AppServer.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            m_AppServer.Stop();
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

        StatusInfoAttribute[] IStatusInfoSource.GetServerStatusMetadata()
        {
            return m_AppServer.GetServerStatusMetadata();
        }

        StatusInfoCollection IStatusInfoSource.CollectServerStatus(StatusInfoCollection nodeStatus)
        {
            return m_AppServer.CollectServerStatus(nodeStatus);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get { return m_AppServer.Name; }
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// Return null, never expire
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" /> property.
        /// </returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" />
        ///   </PermissionSet>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Transfers the system message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageData">The message data.</param>
        public void TransferSystemMessage(string messageType, object messageData)
        {
            m_AppServer.TransferSystemMessage(messageType, messageData);
        }
    }
}
