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
    /// <summary>
    /// It's a protocol like that:
    /// +-------+---+-------------------------------+
    /// |request| l |                               |
    /// | name  | e |    request body               |
    /// |  (4)  | n |                               |
    /// |       |(2)|                               |
    /// +-------+---+-------------------------------+
    /// request name: the name of the request, 4 chars, used for matching the processing command
    /// len: the lenght of request data, two bytes, 0x00 0x02 = 2, 0x01 0x01 = 257
    /// request data: the body of the request
    /// </summary>
    class CustomProtocolServer : AppServer<CustomProtocolSession, BufferedPackageInfo>
    {
        public CustomProtocolServer()
            : base(new DefaultReceiveFilterFactory<MyReceiveFilter, BufferedPackageInfo>())
        {

        }
    }
}
