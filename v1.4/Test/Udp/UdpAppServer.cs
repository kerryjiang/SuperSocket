using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Udp
{
    class UdpAppServer : AppServer<UdpTestSession, MyUdpCommandInfo>
    {
        public UdpAppServer()
            : base(new MyUdpProtocol())
        {

        }
    }
}
