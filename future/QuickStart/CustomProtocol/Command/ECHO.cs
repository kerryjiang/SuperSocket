using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.CustomProtocol.Command
{
    public class ECHO : CommandBase<CustomProtocolSession, BinaryCommandInfo>
    {
        public override void ExecuteCommand(CustomProtocolSession session, BinaryCommandInfo commandInfo)
        {
            session.SendResponse(Encoding.ASCII.GetString(commandInfo.Data) + Environment.NewLine);
        }
    }
}
