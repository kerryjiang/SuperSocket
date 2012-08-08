using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.TelnetServer_Command
{
    public class ADD : CommandBase<AppSession, StringRequestInfo>
    {
        public override void ExecuteCommand(AppSession session, StringRequestInfo requestInfo)
        {
            session.Send(requestInfo.Parameters.Select(p => Convert.ToInt32(p)).Sum().ToString());
        }
    }
}
