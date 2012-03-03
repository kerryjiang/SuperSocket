using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Management.Server.Command
{
    public class STOP : JsonSubCommand<ManagementSession, string>
    {
        protected override void ExecuteJsonCommand(ManagementSession session, string commandInfo)
        {

        }
    }
}
