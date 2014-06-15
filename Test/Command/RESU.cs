using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class RESU : StringCommandBase<TestSession>
    {
        public static string Result { get; private set; }

        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            Result = requestInfo.Body;
        }
    }
}
