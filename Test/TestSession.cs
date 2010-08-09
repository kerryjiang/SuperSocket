using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.Test
{
    public class TestSession : AppSession<TestSession>
    {
        public const string WelcomeMessageFormat = "Welcome to {0}";
        public const string UnknownCommandMessageFormat = "Unknown command: {0}";

        protected override void OnClosed()
        {
            
        }

        public override void SayWelcome()
        {
            SendResponse(string.Format(WelcomeMessageFormat, AppServer.Name));
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }

        public override void HandleUnknownCommand(CommandInfo cmdInfo)
        {
            SendResponse(string.Format(UnknownCommandMessageFormat, cmdInfo.Name));
        }
    }
}
