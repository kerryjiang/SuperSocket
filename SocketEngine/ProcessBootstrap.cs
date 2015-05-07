using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using AnyLog;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    class DefaultBootstrapProcessWrap : DefaultBootstrapAppDomainWrap
    {
        public DefaultBootstrapProcessWrap(IBootstrap bootstrap, IConfigurationSource config, string startupConfigFile)
            : base(bootstrap, config, startupConfigFile)
        {

        }



        protected override IManagedApp CreateWorkItemInstance(string serviceTypeName)
        {
            return new ProcessAppServer(serviceTypeName, GetServerTypeMetadata(serviceTypeName));
        }
    }

    class ProcessBootstrap : AppDomainBootstrap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessBootstrap" /> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public ProcessBootstrap(IConfigurationSource config)
            : base(config)
        {
            var clientChannel = ChannelServices.RegisteredChannels.FirstOrDefault(c => c is IpcClientChannel);

            if(clientChannel == null)
            {
                // Create the channel.
                clientChannel = new IpcClientChannel();
                // Register the channel.
                ChannelServices.RegisterChannel(clientChannel, false);
            }
        }

        protected override IBootstrap CreateBootstrapWrap(IBootstrap bootstrap, IConfigurationSource config, string startupConfigFile)
        {
            return new DefaultBootstrapProcessWrap(bootstrap, config, startupConfigFile);
        }
    }
}
