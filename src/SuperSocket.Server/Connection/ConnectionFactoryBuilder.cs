using System;
using System.Net;
using System.Net.Sockets;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    /// <summary>
    /// Builder for creating connection factories with specified options and initializers.
    /// </summary>
    public class ConnectionFactoryBuilder : IConnectionFactoryBuilder
    {
        /// <summary>
        /// Gets the action used to configure socket options.
        /// </summary>
        public Action<Socket> SocketOptionsSetter { get; }

        /// <summary>
        /// Gets the factory for creating connection stream initializers.
        /// </summary>
        public IConnectionStreamInitializersFactory ConnectionStreamInitializersFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionFactoryBuilder"/> class.
        /// </summary>
        /// <param name="socketOptionsSetter">The setter for configuring socket options.</param>
        /// <param name="connectionStreamInitializersFactory">The factory for creating connection stream initializers.</param>
        public ConnectionFactoryBuilder(SocketOptionsSetter socketOptionsSetter, IConnectionStreamInitializersFactory connectionStreamInitializersFactory)
        {
            SocketOptionsSetter = socketOptionsSetter.Setter;
            ConnectionStreamInitializersFactory = connectionStreamInitializersFactory;
        }

        /// <summary>
        /// Builds a connection factory using the specified listen and connection options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        /// <param name="connectionOptions">The options for the connection.</param>
        /// <returns>A new instance of <see cref="IConnectionFactory"/>.</returns>
        public virtual IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            return new TcpConnectionFactory(listenOptions, connectionOptions, SocketOptionsSetter, ConnectionStreamInitializersFactory);
        }
    }
}