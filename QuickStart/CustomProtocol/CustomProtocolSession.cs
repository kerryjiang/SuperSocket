using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol
{
    public class CustomProtocolSession : AppSession<CustomProtocolSession, BufferedPackageInfo>
    {
        protected override void HandleException(Exception e)
        {

        }
    }
}
