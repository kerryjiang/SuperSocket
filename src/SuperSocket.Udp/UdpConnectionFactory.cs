using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Udp
{
    public class UdpConnectionFactory<TPackageInfo> : IConnectionFactory
    {
        protected IPipelineFilterFactory<TPackageInfo> PipelineFilterFactory;

        public UdpConnectionFactory(IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory)
        {
            this.PipelineFilterFactory = pipelineFilterFactory;
        }

        public Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var connectionInfo = (UdpConnectionInfo)connection;

            var filter = PipelineFilterFactory.Create(connectionInfo.Socket);

            return Task.FromResult<IConnection>(new UdpPipeConnection<TPackageInfo>(connectionInfo.Socket, filter, connectionInfo.ConnectionOptions, connectionInfo.Socket.RemoteEndPoint as IPEndPoint, connectionInfo.SessionIdentifier));
        }
    }
}