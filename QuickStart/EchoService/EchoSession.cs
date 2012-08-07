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
            Send("Welcome to EchoServer!");
        }

        public override void HandleException(Exception e)
        {
            Send("Server side error occurred!");
        }
    }
}
