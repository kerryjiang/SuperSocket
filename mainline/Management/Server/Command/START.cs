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
    /// <summary>
    /// Start command, which is used for starting AppServer instance
    /// </summary>
    public class START : AsyncJsonSubCommand<ManagementSession, string>
    {
        /// <summary>
        /// Executes the async json command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="token">The token.</param>
        /// <param name="commandInfo">The command info.</param>
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
                SendJsonResponse(session, token, new StartResult { Result = false, Message = string.Format("The server instance \"{0}\" doesn't exist", commandInfo) });
                return;
            }

            server.Start();

            SendJsonResponse(session, token, new StartResult { Result = true, ServerInfo = session.AppServer.GetUpdatedCurrentServerInfo() });
        }
    }
}
