using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class PROC : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            session.Send(AppDomain.CurrentDomain.FriendlyName + "," + AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
