using System;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Represents a delegate that handles new connection accept events.
    /// </summary>
    /// <param name="listenOptions">The options for the listener that accepted the connection.</param>
    /// <param name="connection">The newly accepted connection.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    public delegate ValueTask NewConnectionAcceptHandler(ListenOptions listenOptions, IConnection connection);

    /// <summary>
    /// Represents a listener for incoming connections.
    /// </summary>
    public interface IConnectionListener : IDisposable
    {
        /// <summary>
        /// Gets the options for the listener.
        /// </summary>
        ListenOptions Options { get; }

        /// <summary>
        /// Starts the connection listener.
        /// </summary>
        /// <returns><c>true</c> if the listener started successfully; otherwise, <c>false</c>.</returns>
        bool Start();

        /// <summary>
        /// Occurs when a new connection is accepted.
        /// </summary>
        event NewConnectionAcceptHandler NewConnectionAccept;

        /// <summary>
        /// Stops the connection listener asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        Task StopAsync();

        /// <summary>
        /// Gets a value indicating whether the listener is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the factory for creating connections.
        /// </summary>
        IConnectionFactory ConnectionFactory { get; }
    }
}