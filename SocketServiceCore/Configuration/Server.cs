using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Config;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Configuration
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

		[ConfigurationProperty("ip", IsRequired = false)]
		public string Ip
		{
			get { return this["ip"] as string; }
		}

		[ConfigurationProperty("port", IsRequired = true)]
		public int Port
		{
			get { return (int)this["port"]; }
		}

        [ConfigurationProperty("mode", IsRequired = false, DefaultValue = "Sync")]
        public SocketMode Mode
        {
            get { return (SocketMode)this["mode"]; }
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

        [ConfigurationProperty("provider", IsRequired = false)]
        public string Provider
        {
            get { return (string)this["provider"]; }
        }

        [ConfigurationProperty("readTimeOut", IsRequired = false, DefaultValue = 0)]
        public int ReadTimeOut
        {
            get { return (int)this["readTimeOut"]; }
        }

        [ConfigurationProperty("sendTimeOut", IsRequired = false, DefaultValue = 0)]
        public int SendTimeOut
        {
            get { return (int)this["sendTimeOut"]; }
        }

        [ConfigurationProperty("maxConnectionNumber", IsRequired = false, DefaultValue = 1000000)]
        public int MaxConnectionNumber
        {
            get { return (int)this["maxConnectionNumber"]; }
        }
	}
}
