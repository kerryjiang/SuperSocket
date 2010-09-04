using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.BroadcastService.Command
{
    public class CONN : CommandBase<BroadcastSession>
    {
        protected override void Execute(BroadcastSession session, CommandInfo commandData)
        {
            session.DeviceNumber = commandData[0];
            session.AppServer.RegisterNewSession(session);
            session.SendResponse("100 Connected");
        }
    }
}
