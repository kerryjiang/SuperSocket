using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Client;

namespace SuperSocket.Client.Proxy
{
    public class Socks5Connector : ConnectorBase
    {
        protected override ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}