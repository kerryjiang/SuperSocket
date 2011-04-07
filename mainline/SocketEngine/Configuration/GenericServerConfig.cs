using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;
using System.Collections.Specialized;
using System.Configuration;

namespace SuperSocket.SocketEngine.Configuration
{
    public class GenericServerConfig : ConfigurationElementBase, IGenericServerConfig
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }
    }

    [ConfigurationCollection(typeof(GenericServerConfig), AddItemName = "server")] 
    public class GenericServerConfigCollection : GenericConfigurationElementCollection<GenericServerConfig, IGenericServerConfig>
    {

    }
}
