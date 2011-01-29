using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.CustomProtocol
{
    public class CustomProtocolSession : AppSession<CustomProtocolSession, BinaryCommandInfo>
    {
        public override void HandleExceptionalError(Exception e)
        {

        }
    }
}
