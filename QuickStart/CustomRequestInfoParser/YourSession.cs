using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    public class YourSession : AppSession<YourSession>
    {
        protected override void OnSessionStarted()
        {
            Send("Welcome");
        }

        public override void HandleException(Exception e)
        {
            
        }
    }
}
