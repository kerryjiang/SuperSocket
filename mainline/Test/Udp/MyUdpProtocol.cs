using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Udp
{
    class MyUdpProtocol : IRequestFilterFactory<MyUdpRequestInfo>
    {
        public IRequestFilter<MyUdpRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new MyRequestFilter();
        }
    }
}
