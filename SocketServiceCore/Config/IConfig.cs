using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketServiceCore.Config
{
	public interface IConfig : IGetCerticateConfig
	{
		List<IServerConfig> GetServerList();
		
		List<IServiceConfig> GetServiceList();

		ICredentialConfig CredentialConfig { get; }
	}
}
