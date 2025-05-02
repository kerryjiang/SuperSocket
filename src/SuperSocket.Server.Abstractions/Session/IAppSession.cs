using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Represents an application session in the SuperSocket server.
    /// </summary>
    public interface IAppSession
    {
        /// <summary>
        /// Gets the unique identifier for this session.
        /// </summary>
        string SessionID { get; }

        /// <summary>
        /// Gets the time when this session started.
        /// </summary>
        DateTimeOffset StartTime { get; }

        /// <summary>
        /// Gets the time of the last activity in this session.
        /// </summary>
        DateTimeOffset LastActiveTime { get; }

        /// <summary>
        /// Gets the underlying connection of this session.
        /// </summary>
        IConnection Connection { get; }

        /// <summary>
        /// Gets the remote endpoint of the connection.
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the local endpoint of the connection.
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Sends binary data asynchronously using the connection.
        /// </summary>
        /// <param name="data">The binary data to send.</param>
        /// <param name="cancellationToken">The token for canceling the operation.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a sequence of binary data asynchronously using the connection.
        /// </summary>
        /// <param name="data">The sequence of binary data to send.</param>
        /// <param name="cancellationToken">The token for canceling the operation.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        ValueTask SendAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a package asynchronously using the specified package encoder.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package to send.</typeparam>
        /// <param name="packageEncoder">The encoder used to encode the package.</param>
        /// <param name="package">The package to send.</param>
        /// <param name="cancellationToken">The token for canceling the operation.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes the session asynchronously with the specified reason.
        /// </summary>
        /// <param name="reason">The reason for closing the session.</param>
        /// <returns>A task representing the asynchronous close operation.</returns>
        ValueTask CloseAsync(CloseReason reason);

        /// <summary>
        /// Gets the server information associated with this session.
        /// </summary>
        IServerInfo Server { get; }

        /// <summary>
        /// Event triggered when the session is connected.
        /// </summary>
        event AsyncEventHandler Connected;

        /// <summary>
        /// Event triggered when the session is closed.
        /// </summary>
        event AsyncEventHandler<CloseEventArgs> Closed;

        /// <summary>
        /// Gets or sets the data context associated with this session.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        /// Initializes the session with server information and connection.
        /// </summary>
        /// <param name="server">The server information.</param>
        /// <param name="connection">The connection for this session.</param>
        void Initialize(IServerInfo server, IConnection connection);

        /// <summary>
        /// Gets or sets a session item with the specified name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The value of the item.</returns>
        object this[object name] { get; set; }

        /// <summary>
        /// Gets the current state of the session.
        /// </summary>
        SessionState State { get; }

        /// <summary>
        /// Resets the session to its initial state.
        /// </summary>
        void Reset();
    }
}