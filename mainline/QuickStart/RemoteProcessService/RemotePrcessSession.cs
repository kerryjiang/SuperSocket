using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.QuickStart.RemoteProcessService
{
    public class RemotePrcessSession : AppSession<RemotePrcessSession, RemoteProcessServer>
    {
        protected override void OnClosed()
        {
            
        }

        public override void SayWelcome()
        {
            SendResponse("Welcome to use this tool!");
        }

        public override void HandleExceptionalError(Exception e)
        {
            SendResponse("An error has occurred in server side! Error message: " + e.Message + "!");
        }
    }
}
