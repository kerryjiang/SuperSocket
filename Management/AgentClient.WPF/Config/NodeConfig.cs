using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SuperSocket.ServerManager.Client.Config
{
#if !SILVERLIGHT
   [Serializable]
#endif
    public class NodeConfig
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Uri { get; set; }

        [XmlAttribute]
        public string UserName { get; set; }

        [XmlAttribute]
        public string Password { get; set; }
    }
}
