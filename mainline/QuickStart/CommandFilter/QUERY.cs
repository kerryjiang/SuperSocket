using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CommandFilter
{
    [LogTimeCommandFilter]
    public class QUERY : StringCommandBase
    {
        public override void ExecuteCommand(AppSession session, StringRequestInfo commandData)
        {
            //Your code
        }
    }
}
