using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    [Serializable]
    class ServerTypeMetadata
    {
        public StatusInfoAttribute[] StatusInfoMetadata { get; set; }

        public bool IsServerManager { get; set; } 
    }
}
