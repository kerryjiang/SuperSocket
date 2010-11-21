using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.BroadcastService.Command
{
    public class CONN : StringCommandBase<BroadcastSession>
    {
        public override void ExecuteCommand(BroadcastSession session, StringCommandInfo commandData)
        {
            session.DeviceNumber = commandData[0];
            session.AppServer.RegisterNewSession(session);
            session.SendResponse("100 Connected");
        }
    }
}
