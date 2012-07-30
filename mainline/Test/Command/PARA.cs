using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Command
{
    public class PARA : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringRequestInfo commandData)
        {
            foreach (var p in commandData.Parameters)
            {
                Console.WriteLine("S: " + p);
                session.Send(p);
            }
        }
    }
}
