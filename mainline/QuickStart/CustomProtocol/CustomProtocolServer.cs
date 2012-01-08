using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol
{
    /// <summary>
    /// It's a protocol like that:
    /// "SEND 0008 xg^89W(v"
    /// "SEND" is command name, which is 4 chars
    /// "0008" is command data length, which also is 4 chars
    /// "xg^89W(v" is the command data whose lenght is 8
    /// </summary>
    class CustomProtocolServer : AppServer<CustomProtocolSession, BinaryRequestInfo>
    {
        public CustomProtocolServer()
            : base(new DefaultRequestFilterFactory<MyRequestFilter, BinaryRequestInfo>())
        {

        }
    }
}
