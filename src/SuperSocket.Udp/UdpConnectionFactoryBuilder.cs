using System;
using System.Net;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Udp
{
    public class UdpConnectionFactoryBuilder : IConnectionFactoryBuilder
    {
        public IPipelineFilterFactory PipelineFilterFactory { get; }

        public UdpConnectionFactoryBuilder(IPipelineFilterFactory pipelineFilterFactory)
        {
            PipelineFilterFactory = pipelineFilterFactory;
        }

        public IConnectionFactory Build<TPackageInfo>(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            return new UdpConnectionFactory<TPackageInfo>(PipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>);
        }
    }
}