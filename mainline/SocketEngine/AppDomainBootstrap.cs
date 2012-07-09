using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using System.Configuration;
using SuperSocket.Common.Logging;
using SuperSocket.SocketEngine.Configuration;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// AppDomainBootstrap
    /// </summary>
    public sealed class AppDomainBootstrap : MarshalByRefObject, IBootstrap
    {
        class DefaultBootstrapAppDomainWrap : DefaultBootstrap
        {
            public DefaultBootstrapAppDomainWrap(IConfigurationSource config)
                : base(config)
            {

            }

            protected override IWorkItem CreateWorkItemInstance(Type serviceType)
            {
                return new AppDomainAppServer(serviceType);
            }

            protected override ProviderFactoryInfo GetSocketServerFactoryInfo()
            {
                return new ProviderFactoryInfo(ProviderKey.SocketServerFactory, this.GetType().Name, typeof(SocketServerFactory));
            }
        }

        private IBootstrap m_InnerBootstrap;

        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        public IEnumerable<IWorkItem> AppServers
        {
            get { return m_InnerBootstrap.AppServers; }
        }

        /// <summary>
        /// Gets the config.
        /// </summary>
        public IRootConfig Config
        {
            get { return m_InnerBootstrap.Config; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainBootstrap"/> class.
        /// </summary>
        public AppDomainBootstrap(IConfigurationSource config)
        {
            m_InnerBootstrap = new DefaultBootstrapAppDomainWrap(config);
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            return m_InnerBootstrap.Initialize();
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration and config resolver.
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <returns></returns>
        public bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            return m_InnerBootstrap.Initialize(serverConfigResolver);
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration and config resolver.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        public bool Initialize(ILogFactory logFactory)
        {
            return m_InnerBootstrap.Initialize(logFactory);
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        public bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver, ILogFactory logFactory)
        {
            if (logFactory != null)
                throw new Exception("You cannot pass in logFactory, if your isolation level is AppDomain!");

            return m_InnerBootstrap.Initialize(serverConfigResolver, logFactory);
        }

        /// <summary>
        /// Starts this bootstrap.
        /// </summary>
        /// <returns></returns>
        public StartResult Start()
        {
            return m_InnerBootstrap.Start();
        }

        /// <summary>
        /// Stops this bootstrap.
        /// </summary>
        public void Stop()
        {
            m_InnerBootstrap.Stop();
        }

        /// <summary>
        /// Occurs when [performance data collected].
        /// </summary>
        public event EventHandler<PermformanceDataEventArgs> PerformanceDataCollected
        {
            add { m_InnerBootstrap.PerformanceDataCollected += value; }
            remove { m_InnerBootstrap.PerformanceDataCollected -= value; }
        }
    }
}
