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

        public new BroadcastServer AppServer
        {
            get { return (BroadcastServer)base.AppServer; }
        }

        protected override void HandleException(Exception e)
        {
            
        }
    }
}
