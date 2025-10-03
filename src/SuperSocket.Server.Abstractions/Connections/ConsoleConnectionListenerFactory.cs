using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Factory for creating console connection listeners.
    /// </summary>
    public class ConsoleConnectionListenerFactory : IConnectionListenerFactory
    {
        /// <inheritdoc/>
        public IConnectionListener CreateConnectionListener(ListenOptions options, ConnectionOptions connectionOptions, ILoggerFactory loggerFactory)
        {
            connectionOptions.Logger = loggerFactory.CreateLogger(nameof(IConnection));
            var connectionListenerLogger = loggerFactory.CreateLogger(nameof(ConsoleConnectionListener));

            var connectionFactory = new ConsoleConnectionFactory(connectionOptions);
            return new ConsoleConnectionListener(options, connectionFactory, connectionListenerLogger);
        }
    }
}