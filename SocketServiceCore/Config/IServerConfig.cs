using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Config
{
	public interface IServerConfig
	{
		string ServiceName { get; }

		string Ip { get; }

		int Port { get; }

        NameValueConfigurationCollection Parameters { get; }

        string Provider { get; }

        bool Disabled { get; }

        string Name { get; }

        SocketMode Mode { get; }

		bool EnableManagementService { get; }

        int ReadTimeOut { get; }

        int SendTimeOut { get; }

        int MaxConnectionNumber { get; }

        int ReceiveBufferSize { get; }
	}
}
