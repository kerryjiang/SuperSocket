using System;
using System.Net;
using System.Net.Sockets;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    public class ConnectionFactoryBuilder<TPackageInfo> : IConnectionFactoryBuilder
    {
        public Action<Socket> SocketOptionsSetter { get; }

        public IPipelineFilterFactory<TPackageInfo> PipelineFilterFactory { get; }

        public ConnectionFactoryBuilder(SocketOptionsSetter socketOptionsSetter, IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory)
        {
            SocketOptionsSetter = socketOptionsSetter.Setter;
            PipelineFilterFactory = pipelineFilterFactory;
        }

        public virtual IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            return new TcpConnectionFactory<TPackageInfo>(listenOptions, connectionOptions, SocketOptionsSetter, PipelineFilterFactory);
        }
    }
}