using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Udp
{
    class MyUdpProtocol : ICustomProtocol<MyUdpRequestInfo>
    {
        public ICommandReader<MyUdpRequestInfo> CreateCommandReader(IAppServer appServer)
        {
            return new MyCommandReader(appServer);
        }
    }
}
