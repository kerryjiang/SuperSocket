using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.SocksServer
{
    public class SocksSession : AppSession<SocksSession, BinaryCommandInfo>
    {
        public new SocksServer AppServer
        {
            get
            {
                return base.AppServer as SocksServer;
            }
        }

        protected override SocketContext CreateSocketContext()
        {
            return new SocksSocketContext();
        }

        public new SocksSocketContext Context
        {
            get
            {
                return base.Context as SocksSocketContext;
            }
        }
    }
}
