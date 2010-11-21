using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    public class YourSession : AppSession<YourSession>
    {
        protected override void OnClosed()
        {
            
        }

        public override void StartSession()
        {
            SendResponse("Welcome");   
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
