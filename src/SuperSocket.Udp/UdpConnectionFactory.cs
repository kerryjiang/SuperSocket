using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Udp
{
    /// <summary>
    /// Provides a factory for creating UDP connections.
    /// </summary>
    public class UdpConnectionFactory : IConnectionFactory
    {
        /// <summary>
        /// Creates a UDP connection based on the specified connection information.
        /// </summary>
        /// <param name="connection">The connection information.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous creation operation.</returns>
        public Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var connectionInfo = (UdpConnectionInfo)connection;
            
            return Task.FromResult<IConnection>(new UdpPipeConnection(connectionInfo.Socket, connectionInfo.ConnectionOptions, connectionInfo.RemoteEndPoint, connectionInfo.SessionIdentifier));
        }
    }
}