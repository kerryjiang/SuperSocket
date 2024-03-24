using System;
using System.Net.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Kestrel;

public class KestrelPipeConnectionFactoryBuilder : IConnectionFactoryBuilder
{
    private readonly SocketConnectionContextFactory _socketConnectionContextFactory;
    
    private readonly Action<Socket> _socketOptionsSetter;

    public KestrelPipeConnectionFactoryBuilder(SocketConnectionContextFactory socketConnectionContextFactory, SocketOptionsSetter socketOptionsSetter)
    {
        _socketConnectionContextFactory = socketConnectionContextFactory;
        _socketOptionsSetter = socketOptionsSetter.Setter;
    }

    public IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
    {
        return new KestrelPipeConnectionFactory(_socketConnectionContextFactory, listenOptions, connectionOptions, _socketOptionsSetter);
    }
}