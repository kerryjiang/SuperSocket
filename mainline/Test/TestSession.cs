using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test
{
    public class TestSession : AppSession<TestSession>
    {
        public const string WelcomeMessageFormat = "Welcome to {0}";
        public const string UnknownCommandMessageFormat = "Unknown command: {0}";

        public override void StartSession()
        {
            if(AppServer.Config.Mode != SocketMode.Udp)
 	            SendResponse(string.Format(WelcomeMessageFormat, AppServer.Name));
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }

        public override void HandleUnknownCommand(StringCommandInfo cmdInfo)
        {
            SendResponse(string.Format(UnknownCommandMessageFormat, cmdInfo.Key));
        }
    }
}
