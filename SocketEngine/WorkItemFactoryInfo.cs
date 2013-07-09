using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    class WorkItemFactoryInfo
    {
        public string ServerType { get; set; }

        public bool IsServerManager { get; set; }

        public StatusInfoAttribute[] StatusInfoMetadata { get; set; }

        public IServerConfig Config { get; set; }

        public IEnumerable<ProviderFactoryInfo> ProviderFactories { get; set; }

        public ProviderFactoryInfo LogFactory { get; set; }

        public ProviderFactoryInfo SocketServerFactory { get; set; }
    }
}
