using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface IConfig : IRootConfig
    {
        List<IServerConfig> GetServerList();

        List<IServiceConfig> GetServiceList();
    }
}
