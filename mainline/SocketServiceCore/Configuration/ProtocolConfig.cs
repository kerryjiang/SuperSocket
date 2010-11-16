using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore.Configuration
{
    public class ProtocolConfig : ConfigurationElement, IProtocolConfig
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }
    }
}
