using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SuperSocket.Management.Client.Config
{
    [XmlType("ClientApp")]
    public class ClientAppConfig
    {
        [XmlArray]
        public ServerConfig[] Servers { get; set; }

        public string SecurityKey { get; set; }
    }
}
