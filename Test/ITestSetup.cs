using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.Test
{
    public interface ITestSetup
    {
        void Setup(IRootConfig rootConfig, IServerConfig serverConfig);
    }
}
