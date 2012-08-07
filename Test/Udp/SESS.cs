using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Udp
{
    public class SESS : CommandBase<UdpTestSession, MyUdpRequestInfo>
    {
        public override void ExecuteCommand(UdpTestSession session, MyUdpRequestInfo requestInfo)
        {
            session.Send(session.SessionID + " " + requestInfo.Value);
        }
    }
}
