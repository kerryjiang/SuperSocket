using Microsoft.Extensions.Logging;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Defines a factory for creating connection listeners.
    /// </summary>
    public interface IConnectionListenerFactory
    {
        /// <summary>
        /// Creates a connection listener based on the specified options.
        /// </summary>
        /// <param name="options">The options for the listener.</param>
        /// <param name="connectionOptions">The options for the connection.</param>
        /// <param name="loggerFactory">The factory for creating loggers.</param>
        /// <returns>A connection listener.</returns>
        IConnectionListener CreateConnectionListener(ListenOptions options, ConnectionOptions connectionOptions, ILoggerFactory loggerFactory);
    }
}