using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Udp
{
    class MyUdpProtocol : ICustomProtocol<MyUdpCommandInfo>
    {
        public ICommandReader<MyUdpCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new MyCommandReader(appServer);
        }
    }
}
