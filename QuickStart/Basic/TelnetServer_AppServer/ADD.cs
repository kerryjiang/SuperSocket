using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.TelnetServer_AppServer
{
    public class ADD : CommandBase<TelnetSession, StringPackageInfo>
    {
        public override void ExecuteCommand(TelnetSession session, StringPackageInfo requestInfo)
        {
            session.Send(requestInfo.Parameters.Select(p => Convert.ToInt32(p)).Sum().ToString());
        }
    }
}
