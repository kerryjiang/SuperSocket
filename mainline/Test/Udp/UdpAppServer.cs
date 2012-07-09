using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.Common.Logging;

namespace SuperSocket.Test.Udp
{
    class UdpAppServer : AppServer<UdpTestSession, MyUdpRequestInfo>, ITestSetup
    {
        public UdpAppServer()
            : base(new DefaultRequestFilterFactory<MyRequestFilter, MyUdpRequestInfo>())
        {

        }

        void ITestSetup.Setup(IRootConfig rootConfig, IServerConfig serverConfig)
        {
            base.Setup(rootConfig, serverConfig, SocketServerFactory.Instance, null, new ConsoleLogFactory(), null);
        }
    }
}
