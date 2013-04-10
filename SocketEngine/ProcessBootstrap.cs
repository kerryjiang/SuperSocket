using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketEngine
{
    class DefaultBootstrapProcessWrap : DefaultBootstrapAppDomainWrap
    {
        public DefaultBootstrapProcessWrap(IBootstrap bootstrap, IConfigurationSource config, string startupConfigFile)
            : base(bootstrap, config, startupConfigFile)
        {

        }

        protected override IWorkItem CreateWorkItemInstance(string serviceTypeName)
        {
            return new ProcessAppServer(serviceTypeName);
        }
    }

    class ProcessBootstrapProxy : MarshalByRefObject, IBootstrap
    {
        internal static IBootstrap Bootstrap { get; set; }

        public IEnumerable<IWorkItem> AppServers
        {
            get { return Bootstrap.AppServers; }
        }

        public IRootConfig Config
        {
            get { return Bootstrap.Config; }
        }

        public bool Initialize()
        {
            throw new NotSupportedException();
        }

        public bool Initialize(IDictionary<string, System.Net.IPEndPoint> listenEndPointReplacement)
        {
            throw new NotSupportedException();
        }

        public bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            throw new NotSupportedException();
        }

        public bool Initialize(ILogFactory logFactory)
        {
            throw new NotSupportedException();
        }

        public bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver, ILogFactory logFactory)
        {
            throw new NotSupportedException();
        }

        public StartResult Start()
        {
            throw new NotSupportedException();
        }

        public void Stop()
        {
            throw new NotSupportedException();
        }

        public string StartupConfigFile
        {
            get { return Bootstrap.StartupConfigFile; }
        }
    }

    class ProcessBootstrap : AppDomainBootstrap
    {
        internal const string BootstrapIpcPort = "SuperSocket.Bootstrap";

        static ProcessBootstrap()
        {
            // Create the channel.
            var clientChannel = new IpcClientChannel();
            // Register the channel.
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(clientChannel, false);

            var serverChannel = new IpcServerChannel("Bootstrap", BootstrapIpcPort);
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(serverChannel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ProcessBootstrapProxy), "Bootstrap.rem", WellKnownObjectMode.Singleton);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessBootstrap" /> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public ProcessBootstrap(IConfigurationSource config)
            : base(config)
        {
            ProcessBootstrapProxy.Bootstrap = this;
        }

        protected override IBootstrap CreateBootstrapWrap(IBootstrap bootstrap, IConfigurationSource config, string startupConfigFile)
        {
            return new DefaultBootstrapProcessWrap(bootstrap, config, startupConfigFile);
        }
    }
}
