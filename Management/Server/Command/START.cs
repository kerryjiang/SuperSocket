using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.Management.Server.Model;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.SubProtocol;
using SuperSocket.SocketBase.Metadata;

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

            var instanceName = commandInfo;

            var server = session.AppServer.GetServerByName(instanceName);

            if (server == null)
            {
                SendJsonMessage(session, token,
                    new CommandResult
                    {
                        Result = false,
                        Message = string.Format("The server instance \"{0}\" doesn't exist", commandInfo)
                    });
                return;
            }

            if (server.Start())
            {
                var nodeStatus = session.AppServer.CurrentNodeStatus;
                var instance = nodeStatus.InstancesStatus.FirstOrDefault(i => i.Name.Equals(instanceName));
                instance[StatusInfoKeys.IsRunning] = true;
                SendJsonMessage(session, token, new CommandResult { Result = true, NodeStatus = nodeStatus });
            }
            else
            {
                SendJsonMessage(session, token, new CommandResult { Result = false, Message = "Application Error" });
            }
        }
    }
}
