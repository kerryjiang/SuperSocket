using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class PARA : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            foreach (var p in requestInfo.Parameters)
            {
                Console.WriteLine("S: " + p);
                session.Send(p);
            }
        }
    }
}
