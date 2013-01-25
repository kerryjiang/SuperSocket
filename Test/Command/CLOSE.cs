using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Command
{
    public class CLOSE : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringRequestInfo requestInfo)
        {
            session.Close(CloseReason.ServerClosing);
        }
    }
}
