using System;
using System.Text;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase;

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
