using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace PerformanceTestServer.Command
{
    public class TEST : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringCommandInfo commandInfo)
        {
            if (session.AppServer.SendResponseToClient)
            {
                session.SendResponse(commandInfo.Data);
            }
        }
    }
}
