using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class CLOSE : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            if (!string.IsNullOrEmpty(requestInfo.Body))
                session.Send(requestInfo.Body);

            session.Close(CloseReason.ServerClosing);
        }
    }
}
