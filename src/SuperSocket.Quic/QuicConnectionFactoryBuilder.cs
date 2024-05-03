using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Quic
{
    internal class QuicConnectionFactoryBuilder : IConnectionFactoryBuilder
    {
        private readonly IConnectionStreamInitializersFactory _connectionStreamInitializersFactory;

        public QuicConnectionFactoryBuilder(IConnectionStreamInitializersFactory connectionStreamInitializersFactory)
        {
            _connectionStreamInitializersFactory = connectionStreamInitializersFactory;
        }

        public IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            return new QuicConnectionFactory(_connectionStreamInitializersFactory,listenOptions, connectionOptions);
        }
    }
}