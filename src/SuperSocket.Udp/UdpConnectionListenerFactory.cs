using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Udp
{
    class UdpConnectionListenerFactory : IConnectionListenerFactory
    {
        private readonly IConnectionFactoryBuilder _connectionFactoryBuilder;

        private readonly IUdpSessionIdentifierProvider _udpSessionIdentifierProvider;

        private readonly IAsyncSessionContainer _sessionContainer;

        public UdpConnectionListenerFactory(IConnectionFactoryBuilder connectionFactoryBuilder, IUdpSessionIdentifierProvider udpSessionIdentifierProvider, IAsyncSessionContainer sessionContainer)
        {
            _connectionFactoryBuilder = connectionFactoryBuilder;
            _udpSessionIdentifierProvider = udpSessionIdentifierProvider;
            _sessionContainer = sessionContainer;
        }
        
        public IConnectionListener CreateConnectionListener<TPackageInfo>(ListenOptions options, ConnectionOptions connectionOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
        {
            var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;
            connectionOptions.Logger = loggerFactory.CreateLogger(nameof(IConnection));
            var connectionFactoryLogger = loggerFactory.CreateLogger(nameof(UdpConnectionFactory<TPackageInfo>));

            var connectionFactory = _connectionFactoryBuilder.Build(options, connectionOptions);

            return new UdpConnectionListener(options, connectionOptions, connectionFactory, connectionFactoryLogger, _udpSessionIdentifierProvider, _sessionContainer);
        }
    }
}