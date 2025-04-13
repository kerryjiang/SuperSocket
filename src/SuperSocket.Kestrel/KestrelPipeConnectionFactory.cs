namespace SuperSocket.Kestrel;

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

/// <summary>
/// Represents a connection factory for creating connections using Kestrel's <see cref="SocketConnectionContextFactory"/>.
/// </summary>
public class KestrelPipeConnectionFactory : TcpConnectionFactoryBase
{
    private readonly SocketConnectionContextFactory _socketConnectionContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="KestrelPipeConnectionFactory"/> class with the specified context factory, listen options, connection options, and socket options setter.
    /// </summary>
    /// <param name="socketConnectionContextFactory">The factory for creating socket connection contexts.</param>
    /// <param name="listenOptions">The options for listening to incoming connections.</param>
    /// <param name="connectionOptions">The options for managing connections.</param>
    /// <param name="socketOptionsSetter">The setter for configuring socket options.</param>
    public KestrelPipeConnectionFactory(SocketConnectionContextFactory socketConnectionContextFactory, ListenOptions listenOptions, ConnectionOptions connectionOptions, Action<Socket> socketOptionsSetter)
        : base(listenOptions, connectionOptions, socketOptionsSetter, null)
    {
        _socketConnectionContextFactory = socketConnectionContextFactory;
    }

    /// <summary>
    /// Creates a connection using the specified socket and cancellation token.
    /// </summary>
    /// <param name="connection">The socket representing the connection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created connection.</returns>
    public override Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
    {
        var socket = connection as Socket;
        ApplySocketOptions(socket);
        var context = _socketConnectionContextFactory.Create(socket);
        return Task.FromResult<IConnection>(new KestrelPipeConnection(context, ConnectionOptions));
    }
}
