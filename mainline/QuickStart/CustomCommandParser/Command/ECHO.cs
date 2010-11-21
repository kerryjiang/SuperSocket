using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.CustomCommandParser.Command
{
    public class ECHO : StringCommandBase<YourSession>
    {
        public override void ExecuteCommand(YourSession session, StringCommandInfo commandData)
        {
            foreach (var p in commandData.Parameters)
            {
                session.SendResponse(p);
            }
        }
    }
}
