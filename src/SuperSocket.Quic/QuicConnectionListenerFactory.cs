using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Quic
{
    internal class QuicConnectionListenerFactory : IConnectionListenerFactory
    {
        private readonly IConnectionFactoryBuilder _connectionFactoryBuilder;

        public QuicConnectionListenerFactory(IConnectionFactoryBuilder connectionFactoryBuilder)
        {
            _connectionFactoryBuilder = connectionFactoryBuilder;
        }
        
        public IConnectionListener CreateConnectionListener(ListenOptions options, ConnectionOptions connectionOptions, ILoggerFactory loggerFactory)
        {
            connectionOptions.Logger = loggerFactory.CreateLogger(nameof(IConnection));
            var connectionFactoryLogger = loggerFactory.CreateLogger(nameof(QuicConnectionListener));

            var connectionFactory = _connectionFactoryBuilder.Build(options, connectionOptions);

            return new QuicConnectionListener(options,  connectionFactory, connectionFactoryLogger);
        }
    }
}