using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine.Configuration;
using SuperSocket.SocketBase.Config;
using System.Configuration;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Bootstrap Factory
    /// </summary>
    public static class BootstrapFactory
    {
        /// <summary>
        /// Creates the bootstrap.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public static IBootstrap CreateBootstrap(IConfigurationSource config)
        {
            if (config.IsolationMode == IsolationMode.AppDomain)
                return new AppDomainBootstrap(config);
            else
                return new DefaultBootstrap(config);
        }

        /// <summary>
        /// Creates the bootstrap.
        /// </summary>
        /// <param name="configSectionName">Name of the config section.</param>
        /// <returns></returns>
        public static IBootstrap CreateBootstrap(string configSectionName)
        {
            return CreateBootstrap(ConfigurationManager.GetSection(configSectionName) as IConfigurationSource);
        }
    }
}
