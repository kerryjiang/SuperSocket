using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class ECHO : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringCommandInfo commandData)
        {
            Console.WriteLine("R:" + commandData.CommandData);
            session.SendResponse(commandData.CommandData);
        }
    }
}
