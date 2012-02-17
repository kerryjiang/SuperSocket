using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Authentication;

namespace SuperSocket.SocketBase
{
    public class ListenerInfo
    {
        public IPEndPoint EndPoint { get; set; }

        public int BackLog { get; set; }

        public SslProtocols Security { get; set; }
    }
}
