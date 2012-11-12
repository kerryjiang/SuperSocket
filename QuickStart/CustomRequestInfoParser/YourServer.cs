using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    public class YourServer : AppServer<YourSession>
    {
        public YourServer()
            : base(new CommandLineReceiveFilterFactory(Encoding.Default, new CustomRequestInfoParser()))
        {
 
        }
    }
}
