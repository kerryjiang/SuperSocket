using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SuperSocket.Management.Client.Config
{
    [XmlType("Server")]
    public class ServerConfig
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Uri { get; set; }

        [XmlAttribute]
        public string UserName { get; set; }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlArray]
        public string[] Instances { get; set; }
    }
}
