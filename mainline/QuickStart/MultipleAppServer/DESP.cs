using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.MultipleAppServer
{
    public class DESP : StringCommandBase
    {
        public override void ExecuteCommand(AppSession session, StringCommandInfo commandInfo)
        {
            ((MyAppServerA)session.AppServer).DespatchMessage(commandInfo[0], commandInfo[1]);
        }
    }
}
