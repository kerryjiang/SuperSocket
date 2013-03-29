using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine
{
    class ProcessBootstrap : DefaultBootstrap
    {
        public ProcessBootstrap(IConfigurationSource config)
            : base(config)
        {
            
        }
    }
}
