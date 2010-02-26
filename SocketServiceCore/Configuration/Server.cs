using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Config;
using System.Configuration;

namespace GiantSoft.SocketServiceCore.Configuration
{
	public class Server : ConfigurationElement, IServerConfig
	{
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
        }

		[ConfigurationProperty("serviceName", IsRequired = true)]
		public string ServiceName
		{
			get { return this["serviceName"] as string; }
		}

		[ConfigurationProperty("ip", IsRequired = true)]
		public string Ip
		{
			get { return this["ip"] as string; }
		}

		[ConfigurationProperty("port", IsRequired = true)]
		public int Port
		{
			get { return (int)this["port"]; }
		}

        [ConfigurationProperty("disabled", DefaultValue = "false")]
        public bool Disabled
        {
            get { return (bool)this["disabled"]; }
        }

		[ConfigurationProperty("enableManagementService", DefaultValue = "false")]
		public bool EnableManagementService
		{
			get { return (bool)this["enableManagementService"]; }
		}

        [ConfigurationProperty("parameters")]
        public NameValueConfigurationCollection Parameters
        {
            get { return (NameValueConfigurationCollection)this["parameters"]; }
        }

        [ConfigurationProperty("provider", IsRequired = true)]
        public string Provider
        {
            get { return (string)this["provider"]; }
        }
	}
}
