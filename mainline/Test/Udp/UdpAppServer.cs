using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Udp
{
    class UdpAppServer : AppServer<UdpTestSession, MyUdpRequestInfo>
    {
        public UdpAppServer()
            : base(new DefaultRequestFilterFactory<MyRequestFilter, MyUdpRequestInfo>())
        {

        }
    }
}
