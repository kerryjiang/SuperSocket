using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common.Logging;
using SuperSocket.Dlr;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine;

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
