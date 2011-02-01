using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    [ConfigurationCollection(typeof(Service), AddItemName = "service")]   
    public class ServiceCollection : GenericConfigurationElementCollection<Service, IServiceConfig>
    {
    }
}
