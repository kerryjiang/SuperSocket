using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.SocketServiceCore.Config
{
	public interface IConfig : IGetCerticateConfig
	{
		List<IServerConfig> GetServerList();
		
		List<IServiceConfig> GetServiceList();

		ICredentialConfig CredentialConfig { get; }
	}
}
