using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface IConfig : IRootConfig
    {
        IEnumerable<IServerConfig> Servers { get; }

        IEnumerable<IGenericServerConfig> GenericServers { get; }

        IEnumerable<IServiceConfig> Services { get; }
        
        IEnumerable<IConnectionFilterConfig> ConnectionFilters { get; }
    }
}
