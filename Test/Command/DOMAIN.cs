using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class DOMAIN : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            session.Send(AppDomain.CurrentDomain.FriendlyName + "," + AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
