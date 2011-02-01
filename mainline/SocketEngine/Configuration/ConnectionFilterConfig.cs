using System;
using System.Collections.Specialized;
using System.Configuration;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    public class ConnectionFilterConfig : ConfigurationElementBase, IConnectionFilterConfig
    {
        #region IServiceConfig Members
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }
        
        public NameValueCollection Options { get; private set; }
        #endregion
        
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if(Options == null)
            {
                Options = new NameValueCollection();
            }
            
            Options.Add(name, value);
            return true;
        }
    }
    
    [ConfigurationCollection(typeof(ConnectionFilterConfig), AddItemName = "connectionFilter")] 
    public class ConnectionFilterConfigCollection : GenericConfigurationElementCollection<ConnectionFilterConfig, IConnectionFilterConfig>
    {
        
    }
}

