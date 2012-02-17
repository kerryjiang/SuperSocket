using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Configuration;
using SuperSocket.Common;

namespace SuperSocket.SocketEngine.Configuration
{
    public class Listener : ConfigurationElement, IListenerConfig
    {
        [ConfigurationProperty("ip", IsRequired = true)]
        public string Ip
        {
            get { return this["ip"] as string; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        [ConfigurationProperty("backlog", IsRequired = false, DefaultValue = 100)]
        public int Backlog
        {
            get { return (int)this["backlog"]; }
        }

        [ConfigurationProperty("security", IsRequired = false, DefaultValue = "None")]
        public string Security
        {
            get
            {
                return (string)this["security"];
            }
        }
    }

    [ConfigurationCollection(typeof(Listener), AddItemName = "listener")]
    public class ListenerConfigCollection : GenericConfigurationElementCollectionBase<Listener, IListenerConfig>
    {

    }
}
