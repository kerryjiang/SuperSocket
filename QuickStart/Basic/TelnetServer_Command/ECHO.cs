using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.TelnetServer_Command
{
    public class ECHO : CommandBase<AppSession, StringPackageInfo>
    {
        public override void ExecuteCommand(AppSession session, StringPackageInfo requestInfo)
        {
            session.Send(requestInfo.Body);
        }
    }
}
