using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CommandFilter
{
    [LoggedInValidationFilter(Order = 0)]
    [LogTimeCommandFilter(Order = 1)]
    public class QUERY : StringCommandBase<MyAppSession>
    {
        public override void ExecuteCommand(MyAppSession session, StringRequestInfo requestInfo)
        {
            //Your code
        }
    }
}
