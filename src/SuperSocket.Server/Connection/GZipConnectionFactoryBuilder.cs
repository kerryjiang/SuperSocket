using System;
using System.Net;
using System.Net.Sockets;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;

namespace SuperSocket.Server.Connection
{
    public class GZipConnectionFactoryBuilder<TPackageInfo> : ConnectionFactoryBuilder<TPackageInfo>
    {
        public ConnectionFactoryBuilder(SocketOptionsSetter socketOptionsSetter, IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory)
            : base(socketOptionsSetter, pipelineFilterFactory)    
        {
        }

        public virtual IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            return new GZipTcpConnectionFactory<TPackageInfo>(listenOptions, connectionOptions, SocketOptionsSetter, PipelineFilterFactory);
        }
    }
}