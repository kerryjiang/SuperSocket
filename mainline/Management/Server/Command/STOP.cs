using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Management.Shared;
using SuperSocket.Common;

namespace SuperSocket.Management.Server.Command
{
    public class STOP : AsyncJsonSubCommand<ManagementSession, string>
    {
        protected override void ExecuteAsyncJsonCommand(ManagementSession session, string token, string commandInfo)
        {
            if (!session.LoggedIn)
            {
                session.Close();
                return;
            }

            var server = session.AppServer.GetServerByName(commandInfo);

            if (server == null)
            {
                SendJsonResponse(session, token, new StopResult { Result = false, Message = string.Format("The server instance \"{0}\" doesn't exist", commandInfo) });
                return;
            }

            server.Stop();

            SendJsonResponse(session, token, new StartResult { Result = true, ServerInfo = session.AppServer.GetUpdatedCurrentServerInfo() });
        }
    }
}
