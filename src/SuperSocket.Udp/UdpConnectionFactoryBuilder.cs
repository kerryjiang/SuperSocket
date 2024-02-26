using System;
using System.Net;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Udp
{
    public class UdpConnectionFactoryBuilder<TPackageInfo> : IConnectionFactoryBuilder
    {
        public IPipelineFilterFactory<TPackageInfo> PipelineFilterFactory { get; }

        public UdpConnectionFactoryBuilder(IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory)
        {
            PipelineFilterFactory = pipelineFilterFactory;
        }

        public IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            return new UdpConnectionFactory<TPackageInfo>(PipelineFilterFactory);
        }
    }
}