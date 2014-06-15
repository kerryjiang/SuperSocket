using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.ProtoBase;

namespace SuperSocket.Test
{
    public class TestSession : AppSession<TestSession>
    {
        public const string WelcomeMessageFormat = "Welcome to {0}";
        public const string UnknownCommandMessageFormat = "Unknown command: {0}";

        public new TestServer AppServer
        {
            get { return (TestServer)base.AppServer; }
        }

        protected override void OnSessionStarted()
        {
            if (AppServer.SendWelcome)
            {
                if (AppServer.Config.Mode != SocketMode.Udp)
                    Send(string.Format(WelcomeMessageFormat, AppServer.Name));
            }
        }

        protected override void HandleException(Exception e)
        {
            
        }

        protected override void HandleUnknownRequest(StringPackageInfo cmdInfo)
        {
            string response = string.Format(UnknownCommandMessageFormat, cmdInfo.Key);
            Send(response);
        }
    }
}
