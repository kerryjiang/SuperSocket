using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.Test.Command
{
    public class CULT : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringPackageInfo packageInfo)
        {
            session.Send(Thread.CurrentThread.CurrentCulture.Name);
        }
    }
}
