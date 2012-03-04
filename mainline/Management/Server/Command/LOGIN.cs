using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Management.Shared;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;

namespace SuperSocket.Management.Server.Command
{
    public class LOGIN : JsonSubCommand<ManagementSession, LoginInfo>
    {
        protected override void ExecuteJsonCommand(ManagementSession session, LoginInfo commandInfo)
        {
            SendJsonResponse(session, new LoginResult { Result = true, ServerInfo = session.AppServer.CurrentServerInfo });
        }
    }
}
