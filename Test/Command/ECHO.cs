using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.Test.Command
{
    class ECHO : CommandBase<TestSession>
    {
        protected override void Execute(TestSession session, CommandInfo commandData)
        {
            session.SendResponse(commandData.Param);
        }
    }
}
