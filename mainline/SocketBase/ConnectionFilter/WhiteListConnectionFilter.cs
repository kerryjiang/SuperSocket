using System;
using System.Collections.Specialized;
using System.Net;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.ConnectionFilter
{
    public sealed class WhiteListConnectionFilter : TextFileSourceFilter
    {
        public override bool AllowConnect(IPEndPoint remoteAddress)
        {
            return Contains(remoteAddress.Address.ToString());
        }
    }
}

