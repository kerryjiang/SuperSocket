using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol
{
    public class CustomProtocolSession : AppSession<CustomProtocolSession, BinaryRequestInfo>
    {
        public override void HandleException(Exception e)
        {

        }
    }
}
