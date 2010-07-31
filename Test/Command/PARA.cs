using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.Test.Command
{
    public class PARA : CommandBase<TestSession>
    {
        protected override void Execute(TestSession session, CommandInfo commandData)
        {
            foreach (var p in commandData.Parameters)
            {
                Console.WriteLine("S: " + p);
                session.SendResponse(p);
            }
        }
    }
}
