using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.ServerManager.Model;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.SubProtocol;
using SuperSocket.SocketBase.Metadata;
using System.Threading;

namespace SuperSocket.ServerManager.Command
{
    /// <summary>
    /// Stop command, which is used for stopping AppServer instance
    /// </summary>
    public class RESTART : AsyncJsonSubCommand<ManagementSession, string>
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

            if(server.State != SocketBase.ServerState.Running)
            {
                SendJsonMessage(session, token,
                    new CommandResult
                    {
                        Result = false,
                        Message = string.Format("The server instance \"{0}\" is not running now, so you needn't restart it. Try start command instead.", commandInfo)
                    });
                return;
            }

            server.Stop();

            while (server.State != SocketBase.ServerState.NotStarted)
            {
                Thread.Sleep(10);
                //Wating for stop
                //TODO:Timeout
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
