using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SuperSocket.Management.Server.Model;
using SuperSocket.WebSocket.SubProtocol;

namespace SuperSocket.Management.Server.Command
{
    /// <summary>
    /// Login command, which used for login in
    /// </summary>
    public class LOGIN : JsonSubCommand<ManagementSession, LoginInfo>
    {
        /// <summary>
        /// Executes the json command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandInfo">The command info.</param>
        protected override void ExecuteJsonCommand(ManagementSession session, LoginInfo commandInfo)
        {
            var user = session.AppServer.GetUserByName(commandInfo.UserName);

            if (user == null || !user.Password.Equals(commandInfo.Password, StringComparison.OrdinalIgnoreCase))
            {
                SendJsonMessage(session, new LoginResult { Result = false, Message = "Invalid credential!" });
                return;
            }

            session.LoggedIn = true;
            SendJsonMessage(session,
                new LoginResult
                {
                    Result = true,
                    NodeStatus = session.AppServer.CurrentNodeStatus,
                    ServerMetadataSource = session.AppServer.ServerStatusMetadataSource
                });
        }

        private string EncryptPassword(string password)
        {
            return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(password)));
        }
    }
}
