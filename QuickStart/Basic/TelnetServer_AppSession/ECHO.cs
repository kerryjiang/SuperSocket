using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.TelnetServer_AppSession
{
    public class ECHO : CommandBase<TelnetSession, StringPackageInfo>
    {
        public override void ExecuteCommand(TelnetSession session, StringPackageInfo requestInfo)
        {
            session.Send(requestInfo.Body);
        }
    }
}
