using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.SocksServer
{
    public class SocksServer : AppServer<SocksSession, BinaryCommandInfo>
    {
        public SocksServer()
            : base(new SocksProtocol())
        {
            
        }
    }
}
