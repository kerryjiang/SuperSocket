using System;
using System.Net;
using System.Net.Sockets;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    public class ConnectionFactoryBuilder : IConnectionFactoryBuilder
    {
        public Action<Socket> SocketOptionsSetter { get; }

        public IPipelineFilterFactory PipelineFilterFactory { get; }

        public IConnectionStreamInitializersFactory ConnectionStreamInitializersFactory { get; }

        public ConnectionFactoryBuilder(SocketOptionsSetter socketOptionsSetter, IPipelineFilterFactory pipelineFilterFactory, IConnectionStreamInitializersFactory connectionStreamInitializersFactory)
        {
            SocketOptionsSetter = socketOptionsSetter.Setter;
            PipelineFilterFactory = pipelineFilterFactory;
            ConnectionStreamInitializersFactory = connectionStreamInitializersFactory;
        }

        public virtual IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            return new TcpConnectionFactory(listenOptions, connectionOptions, SocketOptionsSetter, ConnectionStreamInitializersFactory);
        }
    }
}