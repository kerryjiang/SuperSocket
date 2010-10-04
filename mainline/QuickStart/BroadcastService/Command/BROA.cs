using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.BroadcastService.Command
{
    public class BROA : CommandBase<BroadcastSession>
    {
        protected override void Execute(BroadcastSession session, CommandInfo commandData)
        {
            string message = commandData.Param;
            session.AppServer.BroadcastMessage(session, message);
            session.SendResponse("101 message broadcasted");
        }
    }
}
