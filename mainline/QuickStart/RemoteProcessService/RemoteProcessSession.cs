using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.RemoteProcessService
{
    public class RemoteProcessSession : AppSession<RemoteProcessSession>
    {
        public new RemoteProcessServer AppServer
        {
            get { return (RemoteProcessServer)base.AppServer; }
        }

        protected override void OnSessionStarted()
        {
            SendResponse("Welcome to use this tool!");
        }

        public override void HandleException(Exception e)
        {
            SendResponse("An error has occurred in server side! Error message: " + e.Message + "!");
        }
    }
}
