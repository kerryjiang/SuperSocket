using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.TelnetServer_AppSession
{
    public class ADD : CommandBase<TelnetSession, StringRequestInfo>
    {
        public override void ExecuteCommand(TelnetSession session, StringRequestInfo requestInfo)
        {
            session.Send(requestInfo.Parameters.Select(p => Convert.ToInt32(p)).Sum().ToString());
        }
    }
}
