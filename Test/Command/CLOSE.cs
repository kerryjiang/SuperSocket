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
            if (!string.IsNullOrEmpty(requestInfo.Body))
                session.Send(requestInfo.Body);

            session.Close(CloseReason.ServerClosing);
        }
    }
}
