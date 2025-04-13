using System;
using System.Net.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Kestrel;

/// <summary>
/// Represents a builder for creating connection factories using Kestrel's <see cref="SocketConnectionContextFactory"/>.
/// </summary>
public class KestrelPipeConnectionFactoryBuilder : IConnectionFactoryBuilder
{
    private readonly SocketConnectionContextFactory _socketConnectionContextFactory;
    
    private readonly Action<Socket> _socketOptionsSetter;

    /// <summary>
    /// Initializes a new instance of the <see cref="KestrelPipeConnectionFactoryBuilder"/> class with the specified context factory and socket options setter.
    /// </summary>
    /// <param name="socketConnectionContextFactory">The factory for creating socket connection contexts.</param>
    /// <param name="socketOptionsSetter">The setter for configuring socket options.</param>
    public KestrelPipeConnectionFactoryBuilder(SocketConnectionContextFactory socketConnectionContextFactory, SocketOptionsSetter socketOptionsSetter)
    {
        _socketConnectionContextFactory = socketConnectionContextFactory;
        _socketOptionsSetter = socketOptionsSetter.Setter;
    }

    /// <summary>
    /// Builds a connection factory using the specified listen options and connection options.
    /// </summary>
    /// <param name="listenOptions">The options for listening to incoming connections.</param>
    /// <param name="connectionOptions">The options for managing connections.</param>
    /// <returns>A connection factory for creating connections.</returns>
    public IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
    {
        return new KestrelPipeConnectionFactory(_socketConnectionContextFactory, listenOptions, connectionOptions, _socketOptionsSetter);
    }
}