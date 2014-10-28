using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocketTest.Command
{
    public class QUIT : StringCommandBase
    {
        public override void ExecuteCommand(AppSession session, StringPackageInfo requestInfo)
        {
            session.Close();
        }
    }
}
