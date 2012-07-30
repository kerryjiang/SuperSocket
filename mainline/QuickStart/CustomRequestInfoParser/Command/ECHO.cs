using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomCommandParser.Command
{
    public class ECHO : StringCommandBase<YourSession>
    {
        public override void ExecuteCommand(YourSession session, StringRequestInfo commandData)
        {
            foreach (var p in commandData.Parameters)
            {
                session.Send(p);
            }
        }
    }
}
