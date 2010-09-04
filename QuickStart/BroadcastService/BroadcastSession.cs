using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.QuickStart.BroadcastService
{
    public class BroadcastSession : AppSession<BroadcastSession, BroadcastServer>
    {
        public string DeviceNumber { get; set; }

        protected override void OnClosed()
        {
            AppServer.RemoveOnlineSession(this);   
        }

        public override void SayWelcome()
        {
            
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
