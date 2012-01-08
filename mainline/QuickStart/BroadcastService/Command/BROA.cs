using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.BroadcastService.Command
{
    public class BROA : StringCommandBase<BroadcastSession>
    {
        public override void ExecuteCommand(BroadcastSession session, StringRequestInfo commandData)
        {
            string message = commandData.Data;
            session.AppServer.BroadcastMessage(session, message);
            session.SendResponse("101 message broadcasted");
        }
    }
}
