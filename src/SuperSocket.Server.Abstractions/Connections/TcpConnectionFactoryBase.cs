using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Provides a base implementation for TCP connection factories.
    /// </summary>
    public abstract class TcpConnectionFactoryBase : IConnectionFactory
    {
        /// <summary>
        /// Gets the options for the listener.
        /// </summary>
        protected ListenOptions ListenOptions { get; }

        /// <summary>
        /// Gets the options for the connection.
        /// </summary>
        protected ConnectionOptions ConnectionOptions { get; }

        /// <summary>
        /// Gets the action to set socket options.
        /// </summary>
        protected Action<Socket> SocketOptionsSetter { get; }

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the collection of connection stream initializers.
        /// </summary>
        protected IEnumerable<IConnectionStreamInitializer> ConnectionStreamInitializers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnectionFactoryBase"/> class.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        /// <param name="connectionOptions">The options for the connection.</param>
        /// <param name="socketOptionsSetter">The action to set socket options.</param>
        /// <param name="connectionStreamInitializersFactory">The factory for creating connection stream initializers.</param>
        public TcpConnectionFactoryBase(
            ListenOptions listenOptions,
            ConnectionOptions connectionOptions,
            Action<Socket> socketOptionsSetter,
            IConnectionStreamInitializersFactory connectionStreamInitializersFactory)
        {
            ListenOptions = listenOptions;
            ConnectionOptions = connectionOptions;
            SocketOptionsSetter = socketOptionsSetter;
            Logger = connectionOptions.Logger;

            ConnectionStreamInitializers = connectionStreamInitializersFactory?.Create(listenOptions);
        }

        /// <summary>
        /// Creates a connection asynchronously.
        /// </summary>
        /// <param name="connection">The connection object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous creation operation.</returns>
        public abstract Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken);

        /// <summary>
        /// Applies socket options to the specified socket.
        /// </summary>
        /// <param name="socket">The socket to configure.</param>
        protected virtual void ApplySocketOptions(Socket socket)
        {
            try
            {
                if (ListenOptions.NoDelay)
                    socket.NoDelay = true;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set NoDelay for the socket.");
            }

            try
            {
                if (ConnectionOptions.ReceiveBufferSize > 0)
                    socket.ReceiveBufferSize = ConnectionOptions.ReceiveBufferSize;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set ReceiveBufferSize for the socket.");
            }

            try
            {
                if (ConnectionOptions.SendBufferSize > 0)
                    socket.SendBufferSize = ConnectionOptions.SendBufferSize;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set SendBufferSize for the socket.");
            }

            try
            {
                if (ConnectionOptions.ReceiveTimeout > 0)
                    socket.ReceiveTimeout = ConnectionOptions.ReceiveTimeout;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set ReceiveTimeout for the socket.");
            }

            try
            {
                if (ConnectionOptions.SendTimeout > 0)
                    socket.SendTimeout = ConnectionOptions.SendTimeout;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set SendTimeout for the socket.");
            }

            try
            {
                SocketOptionsSetter?.Invoke(socket);
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to run socketOptionSetter for the socket.");
            }
        }
    }
}