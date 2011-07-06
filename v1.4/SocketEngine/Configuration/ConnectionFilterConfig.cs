using System;
using System.Collections.Specialized;
using System.Configuration;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    public class ConnectionFilterConfig : ConfigurationElementBase, IConnectionFilterConfig
    {
        #region IConnectionFilterConfig Members
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }
        #endregion
    }
    
    [ConfigurationCollection(typeof(ConnectionFilterConfig), AddItemName = "connectionFilter")] 
    public class ConnectionFilterConfigCollection : GenericConfigurationElementCollection<ConnectionFilterConfig, IConnectionFilterConfig>
    {
        
    }
}

