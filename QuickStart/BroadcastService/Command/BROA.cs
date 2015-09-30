using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.BroadcastService.Command
{
    public class BROA : StringCommandBase<BroadcastSession>
    {
        public override void ExecuteCommand(BroadcastSession session, StringPackageInfo requestInfo)
        {
            string message = requestInfo.Body;
            session.AppServer.BroadcastMessage(session, message);
            session.Send("101 message broadcasted");
        }
    }
}
