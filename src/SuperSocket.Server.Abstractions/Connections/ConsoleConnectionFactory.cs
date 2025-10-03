using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Factory for creating console connections.
    /// </summary>
    public class ConsoleConnectionFactory : IConnectionFactory
    {
        private readonly ConnectionOptions _connectionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionOptions">The connection options.</param>
        public ConsoleConnectionFactory(ConnectionOptions connectionOptions)
        {
            _connectionOptions = connectionOptions;
        }

        /// <summary>
        /// Creates a console connection.
        /// </summary>
        /// <param name="connection">The connection object (ignored for console connections).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous creation operation.</returns>
        public Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var consoleConnection = new ConsoleConnection(_connectionOptions);
            return Task.FromResult<IConnection>(consoleConnection);
        }
    }
}