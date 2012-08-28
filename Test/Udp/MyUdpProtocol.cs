using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using System.Net;

namespace SuperSocket.Test.Udp
{
    class MyUdpProtocol : IRequestFilterFactory<MyUdpRequestInfo>
    {
        public IRequestFilter<MyUdpRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new MyRequestFilter();
        }
    }
}
