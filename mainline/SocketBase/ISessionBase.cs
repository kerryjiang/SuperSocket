using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SuperSocket.SocketBase
{
    public interface ISessionBase
    {
        string SessionID { get; }
        string IdentityKey { get; }
        IPEndPoint RemoteEndPoint { get; }
    }
}
