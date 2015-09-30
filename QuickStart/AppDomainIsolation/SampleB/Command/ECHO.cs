using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.AppDomainIsolation.SampleB.Command
{
    public class ECHO : StringCommandBase
    {
        public override void ExecuteCommand(AppSession session, StringPackageInfo requestInfo)
        {
            session.Send(requestInfo.Body);
        }
    }
}
