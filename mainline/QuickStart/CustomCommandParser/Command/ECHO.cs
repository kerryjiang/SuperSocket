using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.CustomCommandParser.Command
{
    public class ECHO : CommandBase<YourSession>
    {
        protected override void Execute(YourSession session, CommandInfo commandData)
        {
            foreach (var p in commandData.Parameters)
            {
                session.SendResponse(p);
            }
        }
    }
}
