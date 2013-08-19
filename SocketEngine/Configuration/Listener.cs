using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Configuration;
using SuperSocket.Common;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Listener configuration
    /// </summary>
    public class Listener : ConfigurationElement, IListenerConfig
    {
        /// <summary>
        /// Gets the ip of listener
        /// </summary>
        [ConfigurationProperty("ip", IsRequired = true)]
        public string Ip
        {
            get { return this["ip"] as string; }
        }

        /// <summary>
        /// Gets the port of listener
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        /// <summary>
        /// Gets the backlog.
        /// </summary>
        [ConfigurationProperty("backlog", IsRequired = false, DefaultValue = 100)]
        public int Backlog
        {
            get { return (int)this["backlog"]; }
        }

        /// <summary>
        /// Gets the security option, None/Default/Tls/Ssl/...
        /// </summary>
        [ConfigurationProperty("security", IsRequired = false)]
        public string Security
        {
            get
            {
                return (string)this["security"];
            }
        }
    }

    /// <summary>
    /// Listener configuration collection
    /// </summary>
    [ConfigurationCollection(typeof(Listener))]
    public class ListenerConfigCollection : GenericConfigurationElementCollectionBase<Listener, IListenerConfig>
    {

    }
}
