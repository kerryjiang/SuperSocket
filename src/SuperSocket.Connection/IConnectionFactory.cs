using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a factory for creating connections.
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// Creates a new connection asynchronously.
        /// </summary>
        /// <param name="connection">The underlying connection object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created connection.</returns>
        Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken);
    }
}