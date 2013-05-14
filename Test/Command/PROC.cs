using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System.Diagnostics;

namespace SuperSocket.Test.Command
{
    public class PROC : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringRequestInfo requestInfo)
        {
            session.Send(Process.GetCurrentProcess().ProcessName);
        }
    }
}
