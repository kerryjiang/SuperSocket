using System;
using System.Collections.Specialized;
using System.Net;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.ConnectionFilter
{
    public sealed class BlackListConnectionFilter : TextFileSourceFilter
    {
         public override bool AllowConnect(IPEndPoint remoteAddress)
         {
            return !Contains(remoteAddress.Address.ToString());
         }
    }
}

