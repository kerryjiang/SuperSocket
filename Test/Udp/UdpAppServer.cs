using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Dlr;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Udp
{
    class UdpAppServer : AppServer<UdpTestSession, MyUdpRequestInfo>, ITestSetup
    {
        public UdpAppServer()
            : base(new DefaultReceiveFilterFactory<MyReceiveFilter, MyUdpRequestInfo>())
        {

        }

        void ITestSetup.Setup(IRootConfig rootConfig, IServerConfig serverConfig)
        {
            base.Setup(rootConfig, serverConfig, null, null, new ConsoleLogFactory(), null);
        }
    }
}
