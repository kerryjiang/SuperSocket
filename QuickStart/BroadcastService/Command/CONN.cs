using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.BroadcastService.Command
{
    public class CONN : StringCommandBase<BroadcastSession>
    {
        public override void ExecuteCommand(BroadcastSession session, StringRequestInfo requestInfo)
        {
            session.DeviceNumber = requestInfo[0];
            session.AppServer.RegisterNewSession(session);
            session.Send("100 Connected");
        }
    }
}
