using System;
using System.Reflection;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using System.IO;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// AppDomainAppServer
    /// </summary>
    partial class AppDomainAppServer : IsolationAppServer
    {
        private AppDomain m_HostDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainAppServer" /> class.
        /// </summary>
        /// <param name="serverTypeName">Name of the server type.</param>
        /// <param name="serverStatusMetadata">The server status metadata.</param>
        public AppDomainAppServer(string serverTypeName, StatusInfoAttribute[] serverStatusMetadata)
            : base(serverTypeName, serverStatusMetadata)
        {

        }

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>
        /// return true if start successfull, else false
        /// </returns>
        protected override IWorkItemBase Start()
        {
            IWorkItem appServer;

            try
            {
                m_HostDomain = CreateHostAppDomain();

                m_HostDomain.SetData(typeof(IsolationMode).Name, IsolationMode.AppDomain);

                var marshalServerType = typeof(MarshalAppServer);

                appServer = (IWorkItem)m_HostDomain.CreateInstanceAndUnwrap(marshalServerType.Assembly.FullName,
                        marshalServerType.FullName,
                        true,
                        BindingFlags.CreateInstance,
                        null,
                        new object[] { ServerTypeName },
                        null,
                        new object[0]);

                if (!appServer.Setup(Bootstrap, ServerConfig, Factories))
                {
                    OnExceptionThrown(new Exception("Failed to setup MarshalAppServer"));
                    return null;
                }

                if (!appServer.Start())
                {
                    OnExceptionThrown(new Exception("Failed to start MarshalAppServer"));
                    return null;
                }

                m_HostDomain.DomainUnload += new EventHandler(m_HostDomain_DomainUnload);

                return appServer;
            }
            catch (Exception e)
            {
                if (m_HostDomain != null)
                {
                    AppDomain.Unload(m_HostDomain);
                    m_HostDomain = null;
                }

                OnExceptionThrown(e);
                return null;
            }
        }

        void m_HostDomain_DomainUnload(object sender, EventArgs e)
        {
            OnStopped();
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            m_HostDomain = null;
        }

        protected override void Stop()
        {
            if (m_HostDomain != null)
            {
                AppDomain.Unload(m_HostDomain);
            }
        }
    }
}
