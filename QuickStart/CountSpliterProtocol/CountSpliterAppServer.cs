using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CountSpliterProtocol
{
    public class CountSpliterAppServer : AppServer
    {
        public CountSpliterAppServer()
            : base(new CountSpliterRequestFilterFactory<MyRequetFilter>())
        {
            
        }
    }
}
