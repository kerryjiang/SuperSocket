using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.QuickStart.EchoService
{
    public class EchoSession : AppSession<EchoSession>
    {
        protected override void OnClosed()
        {
            
        }

        public override void SayWelcome()
        {
            SendResponse("Welcome to EchoServer!");
        }

        public override void HandleExceptionalError(Exception e)
        {
            SendResponse("Server side error occurred!");
        }
    }
}
