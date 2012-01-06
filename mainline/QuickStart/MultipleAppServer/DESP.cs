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
        public override void ExecuteCommand(AppSession session, StringRequestInfo requestInfo)
        {
            ((MyAppServerA)session.AppServer).DespatchMessage(requestInfo[0], requestInfo[1]);
        }
    }
}
