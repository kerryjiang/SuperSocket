using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.CommandFilter
{
    [LogTimeCommandFilter]
    public class QUERY : StringCommandBase
    {
        public override void ExecuteCommand(AppSession session, StringCommandInfo commandData)
        {
            //Your code
        }
    }
}
