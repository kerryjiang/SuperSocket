using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.SocksServer.Command
{
    public class DATA : CommandBase<SocksSession, BinaryCommandInfo>
    {
        public override void ExecuteCommand(SocksSession session, BinaryCommandInfo commandInfo)
        {
            session.SendDataToTargetSocket(commandInfo.Data);
        }
    }
}
