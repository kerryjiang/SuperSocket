using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.MultipleAppServer
{
    public class DESP : StringCommandBase
    {
        public override void ExecuteCommand(AppSession session, StringPackageInfo requestInfo)
        {
            ((MyAppServerA)session.AppServer).DespatchMessage(requestInfo[0], requestInfo[1]);
        }
    }
}
