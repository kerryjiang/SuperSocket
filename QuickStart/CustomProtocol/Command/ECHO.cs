using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol.Command
{
    public class ECHO : CommandBase<CustomProtocolSession, BinaryRequestInfo>
    {
        public override void ExecuteCommand(CustomProtocolSession session, BinaryRequestInfo requestInfo)
        {
            session.Send(Encoding.ASCII.GetString(requestInfo.Body) + Environment.NewLine);
        }
    }
}
