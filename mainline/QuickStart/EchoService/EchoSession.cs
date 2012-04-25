using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.EchoService
{
    public class EchoSession : AppSession<EchoSession>
    {
        protected override void OnSessionStarted()
        {
            SendResponse("Welcome to EchoServer!");
        }

        public override void HandleException(Exception e)
        {
            SendResponse("Server side error occurred!");
        }
    }
}
