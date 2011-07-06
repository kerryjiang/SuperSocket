using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class PARA : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringCommandInfo commandData)
        {
            foreach (var p in commandData.Parameters)
            {
                Console.WriteLine("S: " + p);
                session.SendResponse(p);
            }
        }
    }
}
