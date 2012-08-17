using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CountSpliterProtocol.Command
{
    public class ECHO : CommandBase<AppSession, StringRequestInfo>
    {
        public override void ExecuteCommand(AppSession session, StringRequestInfo requestInfo)
        {
            for (var i = 0; i < requestInfo.Parameters.Length; i++)
            {
                session.Send(requestInfo.Parameters[i]);
            }
        }
    }
}
