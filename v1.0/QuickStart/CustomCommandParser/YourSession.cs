using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    public class YourSession : AppSession<YourSession, YourServer>
    {
        protected override void OnClosed()
        {
            
        }

        public override void SayWelcome()
        {
            SendResponse("Welcome");   
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
