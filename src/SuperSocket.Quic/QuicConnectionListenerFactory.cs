using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Quic;

internal class QuicConnectionListenerFactory : IConnectionListenerFactory
{
    private readonly QuicTransportOptions _quicTransportOptions;
    private readonly IConnectionFactoryBuilder _connectionFactoryBuilder;

    public QuicConnectionListenerFactory(IConnectionFactoryBuilder connectionFactoryBuilder,
        IOptions<QuicTransportOptions> options)
    {
        _connectionFactoryBuilder = connectionFactoryBuilder;
        _quicTransportOptions = options.Value;
    }

    public IConnectionListener CreateConnectionListener(ListenOptions options, ConnectionOptions connectionOptions,
        ILoggerFactory loggerFactory)
    {
        connectionOptions.Logger = loggerFactory.CreateLogger(nameof(IConnection));
        var connectionFactoryLogger = loggerFactory.CreateLogger(nameof(QuicConnectionListener));

        var connectionFactory = _connectionFactoryBuilder.Build(options, connectionOptions);

        return new QuicConnectionListener(options, _quicTransportOptions, connectionFactory,
            connectionFactoryLogger);
    }
}