using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.BroadcastService
{
    public class BroadcastSession : AppSession<BroadcastSession>
    {
        public string DeviceNumber { get; set; }

        protected override void OnClosed()
        {
            AppServer.RemoveOnlineSession(this);   
        }

        public new BroadcastServer AppServer
        {
            get { return (BroadcastServer)base.AppServer; }
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
