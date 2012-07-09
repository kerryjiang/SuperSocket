using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketEngine
{
    class WorkItemFactoryInfo
    {
        public Type ServiceType { get; set; }

        public IServerConfig Config { get; set; }

        public IEnumerable<ProviderFactoryInfo> ProviderFactories { get; set; }
    }
}
