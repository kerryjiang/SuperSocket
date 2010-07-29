using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    public class YourServer : AppServer<YourSession>
    {
        public YourServer() : base()
        {
            this.CommandParser = new CustomCommandParser();
            this.CommandParameterParser = new Base64CommandParameterParser();
        }
    }
}
