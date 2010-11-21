using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface IConfig
    {
        List<IServerConfig> GetServerList();

        List<IServiceConfig> GetServiceList();

        List<IProtocolConfig> GetProtocolList();

        ICredentialConfig CredentialConfig { get; }

        string ConsoleBaseAddress { get; }

        bool IndependentLogger { get; }
    }
}
