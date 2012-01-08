using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol.Command
{
    public class ECHO : CommandBase<CustomProtocolSession, BinaryRequestInfo>
    {
        public override void ExecuteCommand(CustomProtocolSession session, BinaryRequestInfo commandInfo)
        {
            session.SendResponse(Encoding.ASCII.GetString(commandInfo.Data) + Environment.NewLine);
        }
    }
}
