using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using GiantSoft.SocketServiceCore.Config;

namespace GiantSoft.SocketServiceCore.Configuration
{
	public class Service : ConfigurationElement, IServiceConfig
	{
		#region IServiceConfig Members

		[ConfigurationProperty("serviceName", IsRequired = true)]
		public string ServiceName
		{
			get { return this["serviceName"] as string; }
		}

        [ConfigurationProperty("baseAssembly", IsRequired = true)]
        public string BaseAssembly
        {
            get { return this["baseAssembly"] as string; }
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
