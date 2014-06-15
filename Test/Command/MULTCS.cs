using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class MULTCS : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            session.Send((Convert.ToInt32(requestInfo[0]) * Convert.ToInt32(requestInfo[1])).ToString());
        }
    }
}
