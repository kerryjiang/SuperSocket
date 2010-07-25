using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.Test
{
    class TestSession : AppSession<TestSession>
    {
        public const string WelcomeMessageFormat = "Welcome to {0}";

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
    }
}
