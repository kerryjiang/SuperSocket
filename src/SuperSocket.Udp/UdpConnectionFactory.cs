using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Udp
{
    public class UdpConnectionFactory : IConnectionFactory
    {
        public Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var connectionInfo = (UdpConnectionInfo)connection;
            
            return Task.FromResult<IConnection>(new UdpPipeConnection(connectionInfo.Socket, connectionInfo.ConnectionOptions, connectionInfo.RemoteEndPoint, connectionInfo.SessionIdentifier));
        }
    }
}