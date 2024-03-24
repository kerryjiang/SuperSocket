namespace SuperSocket.Kestrel;

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

public class KestrelPipeConnectionFactory : TcpConnectionFactoryBase
{
    private readonly SocketConnectionContextFactory _socketConnectionContextFactory;

    public KestrelPipeConnectionFactory(SocketConnectionContextFactory socketConnectionContextFactory, ListenOptions listenOptions, ConnectionOptions connectionOptions, Action<Socket> socketOptionsSetter)
        : base(listenOptions, connectionOptions, socketOptionsSetter, null)
    {
        _socketConnectionContextFactory = socketConnectionContextFactory;
    }

    public override Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
    {
        var socket = connection as Socket;
        ApplySocketOptions(socket);
        var context = _socketConnectionContextFactory.Create(socket);
        return Task.FromResult<IConnection>(new KestrelPipeConnection(context, ConnectionOptions));
    }
}
