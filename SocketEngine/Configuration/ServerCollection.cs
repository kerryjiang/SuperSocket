using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Server configuration collection
    /// </summary>
    [ConfigurationCollection(typeof(Server), AddItemName = "server")] 
    public class ServerCollection : GenericConfigurationElementCollection<Server, IServerConfig>
    {
    }
}
