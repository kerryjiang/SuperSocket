using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Command
{
    public class RESU : StringCommandBase<TestSession>
    {
        public static string Result { get; private set; }

        public override void ExecuteCommand(TestSession session, StringRequestInfo requestInfo)
        {
            Result = requestInfo.Body;
        }
    }
}
