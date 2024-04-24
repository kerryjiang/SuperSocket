using System;
using System.Net.Sockets;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.IOCP.ConnectionFactory;

public class TcpPipeIocpConnectionFactoryBuilder : IConnectionFactoryBuilder
{
    private readonly Action<Socket> _socketOptionsSetter;

    public TcpPipeIocpConnectionFactoryBuilder(SocketOptionsSetter socketOptionsSetter)
    {
        _socketOptionsSetter = socketOptionsSetter.Setter;
    }
    
    public IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
    {
        return new TcpPipeIocpConnectionFactory(listenOptions,connectionOptions,_socketOptionsSetter);
    }
}