using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore.Configuration
{
    public class Service : ConfigurationElement, IServiceConfig
    {
        #region IServiceConfig Members

        [ConfigurationProperty("serviceName", IsRequired = true)]
        public string ServiceName
        {
            get { return this["serviceName"] as string; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }


        [ConfigurationProperty("providers")]
        public NameValueConfigurationCollection Providers
        {
            get { return (NameValueConfigurationCollection)this["providers"]; }
        }

        [ConfigurationProperty("disabled", DefaultValue = "false")]
        public bool Disabled
        {
            get { return (bool)this["disabled"]; }
        }

        #endregion
    }
}
