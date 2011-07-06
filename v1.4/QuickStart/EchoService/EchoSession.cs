using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.EchoService
{
    public class EchoSession : AppSession<EchoSession>
    {
        public override void StartSession()
        {
            SendResponse("Welcome to EchoServer!");
        }

        public override void HandleExceptionalError(Exception e)
        {
            SendResponse("Server side error occurred!");
        }
    }
}
