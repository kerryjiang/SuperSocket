using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Udp
{
    public class SESS : CommandBase<UdpTestSession, MyUdpCommandInfo>
    {
        public override void ExecuteCommand(UdpTestSession session, MyUdpCommandInfo commandInfo)
        {
            session.SendResponse(session.IdentityKey + " " + commandInfo.Value);
        }
    }
}
