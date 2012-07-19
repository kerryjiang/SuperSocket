using System;
using System.Reflection;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using System.IO;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// AppDomainAppServer
    /// </summary>
    public class AppDomainAppServer : IWorkItem
    {
        private IWorkItem m_AppServer;

        private string m_ServiceTypeName;

        private IBootstrap m_Bootstrap;

        private IServerConfig m_ServerConfig;

        private ProviderFactoryInfo[] m_Factories;

        private AppDomain m_HostDomain;

        private const string m_WorkingDir = "WorkRoot";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainAppServer"/> class.
        /// </summary>
        /// <param name="serviceTypeName">Name of the service type.</param>
        public AppDomainAppServer(string serviceTypeName)
        {
            m_ServiceTypeName = serviceTypeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainAppServer"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        public AppDomainAppServer(Type serviceType)
        {
            m_ServiceTypeName = serviceType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Gets the name of the server instance.
        /// </summary>
        public string Name
        {
            get { return m_ServerConfig.Name; }
        }

        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="bootstrap">The bootstrap.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        public bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories)
        {
            m_Bootstrap = bootstrap;
            m_ServerConfig = config;
            m_Factories = factories;
            return true;
        }

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>
        /// return true if start successfull, else false
        /// </returns>
        public bool Start()
        {
            try
            {
                var currentDomain = AppDomain.CurrentDomain;
                var marshalServerType = typeof(MarshalAppServer);

                var workingDir = Path.Combine(Path.Combine(currentDomain.BaseDirectory, m_WorkingDir), Name);

                if (!Directory.Exists(workingDir))
                    Directory.CreateDirectory(workingDir);

                m_HostDomain = AppDomain.CreateDomain(m_ServerConfig.Name, currentDomain.Evidence, new AppDomainSetup
                    {
                        ApplicationName = m_ServerConfig.Name,
                        ApplicationBase = workingDir,
                        ConfigurationFile = m_Bootstrap.StartupConfigFile
                    });

                var assemblyImportType = typeof(AssemblyImport);

                var assemblyImport = (AssemblyImport)m_HostDomain.CreateInstanceFromAndUnwrap(assemblyImportType.Assembly.CodeBase,
                        assemblyImportType.FullName,
                        true,
                        BindingFlags.CreateInstance,
                        null,
                        new object[] { currentDomain.BaseDirectory },
                        null,
                        new object[0]);

                m_AppServer = (IWorkItem)m_HostDomain.CreateInstanceAndUnwrap(marshalServerType.Assembly.FullName,
                        marshalServerType.FullName,
                        true,
                        BindingFlags.CreateInstance,
                        null,
                        new object[] { m_ServiceTypeName },
                        null,
                        new object[0]);

                if (!m_AppServer.Setup(m_Bootstrap, m_ServerConfig, m_Factories))
                    throw new Exception("Failed tp setup MarshalAppServer");
            }
            catch (Exception)
            {
                if (m_HostDomain != null)
                {
                    AppDomain.Unload(m_HostDomain);
                    m_HostDomain = null;
                }

                if (m_AppServer != null)
                {
                    m_AppServer = null;
                }

                throw;
            }

            return m_AppServer.Start();
        }

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        public void Stop()
        {
            try
            {
                m_AppServer.Stop();
                m_AppServer = null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                AppDomain.Unload(m_HostDomain);
                m_HostDomain = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning
        {
            get
            {
                if (m_AppServer == null)
                    return false;

                return m_AppServer.IsRunning;
            }
        }


        /// <summary>
        /// Gets the total session count.
        /// </summary>
        public int SessionCount
        {
            get
            {
                if (m_AppServer == null)
                    return 0;

                return m_AppServer.SessionCount;
            }
        }
    }
}
