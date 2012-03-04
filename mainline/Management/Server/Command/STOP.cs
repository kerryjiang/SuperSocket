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
    public class STOP : JsonSubCommand<ManagementSession, string>
    {
        protected override void ExecuteJsonCommand(ManagementSession session, string commandInfo)
        {
            var server = session.AppServer.GetServerByName(commandInfo);

            if (server == null)
            {
                SendJsonResponse(session, new StopResult { Result = false, Message = string.Format("The server instance \"{0}\" doesn't exist", commandInfo) });
                return;
            }

            Async.Run(StopServer, new { Session = session, Server = server, Token = session.CurrentToken });
        }

        private void StopServer(object state)
        {
            var param = (dynamic)state;

            var session = param.Session;
            var token = param.Token;
            var server = param.Server;

            server.Stop();

            SendJsonResponseWithToken(session, token, new StartResult { Result = true, ServerInfo = session.AppServer.CurrentServerInfo });
        }
    }
}
