using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Config
{
	public interface IServiceConfig
	{
		string ServiceName { get; }		

        string BaseAssembly { get; }

        NameValueConfigurationCollection Providers { get; }

        bool Disabled { get; }
	}
}
