using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    public enum SocketMode
    {
        [Obsolete("Please use SocketMode.Tcp instead", false)]
        Async,
        Tcp,
        Udp
    }
}
