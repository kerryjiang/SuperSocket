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
            session.Send(new ArraySegment<byte>(Encoding.UTF8.GetBytes(session.SessionID + " " + requestInfo.Value)));
        }
    }
}
