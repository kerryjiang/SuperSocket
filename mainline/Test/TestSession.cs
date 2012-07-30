using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test
{
    public class TestSession : AppSession<TestSession>
    {
        public const string WelcomeMessageFormat = "Welcome to {0}";
        public const string UnknownCommandMessageFormat = "Unknown command: {0}";

        protected override void OnSessionStarted()
        {
            if(AppServer.Config.Mode != SocketMode.Udp)
                Send(string.Format(WelcomeMessageFormat, AppServer.Name));
        }

        public override void HandleException(Exception e)
        {
            
        }

        public override void HandleUnknownRequest(StringRequestInfo cmdInfo)
        {
            string response = string.Format(UnknownCommandMessageFormat, cmdInfo.Key);
            Send(response);
        }
    }
}
