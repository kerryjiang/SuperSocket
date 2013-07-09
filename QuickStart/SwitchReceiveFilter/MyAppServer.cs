using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.SwitchReceiveFilter
{
    public class MyAppServer : AppServer
    {
        public MyAppServer()
            : base(new DefaultReceiveFilterFactory<SwitchReceiveFilter, StringRequestInfo>())
        {

        }
    }
}
