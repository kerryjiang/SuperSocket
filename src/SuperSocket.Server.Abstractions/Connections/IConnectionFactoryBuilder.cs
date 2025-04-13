using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Defines a builder for creating connection factories.
    /// </summary>
    public interface IConnectionFactoryBuilder
    {
        /// <summary>
        /// Builds a connection factory based on the specified listen and connection options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        /// <param name="connectionOptions">The options for the connection.</param>
        /// <returns>A connection factory.</returns>
        IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions);
    }
}