using System.IO.Pipelines;
using System.Net.Sockets;
using SuperSocket.Connection;
using SuperSocket.IOCP.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SwatchSocket.IOCP.Connection;

namespace SuperSocket.IOCP.ConnectionFactory;

public class TcpPipeIocpConnectionFactory
    : TcpConnectionFactoryBase
{
    private int _settingsIndex;
    private readonly QueueSettings[] _settings;
    private readonly int _settingsCount;
    private readonly ConnectionOptions _connectionOptions;

    public TcpPipeIocpConnectionFactory(
        ListenOptions listenOptions,
        ConnectionOptions connectionOptions,
        Action<Socket> socketOptionsSetter) : base(listenOptions,
        connectionOptions, socketOptionsSetter,
        null)
    {
        _settings = LoadQueueSettings();
        _settingsCount = _settings.Length;
        _connectionOptions = connectionOptions;
    }

    protected virtual QueueSettings[] LoadQueueSettings()
    {
        return QueueSettings.Default;
    }

    public override Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
    {
        var socket = connection as Socket;

        ArgumentNullException.ThrowIfNull(socket);

        ApplySocketOptions(socket);

        QueueSettings setting = _settings[Interlocked.Increment(ref _settingsIndex) % (long)_settingsCount];

        var connectionOptions = new ConnectionOptions
        {
            SendBufferSize = _connectionOptions.SendBufferSize,
            SendTimeout = _connectionOptions.SendTimeout,
            ReceiveBufferSize = _connectionOptions.ReceiveBufferSize,
            Logger = _connectionOptions.Logger,
            MaxPackageLength = _connectionOptions.MaxPackageLength,
            ReadAsDemand = _connectionOptions.ReadAsDemand,
            ReceiveTimeout = _connectionOptions.ReceiveTimeout,
            Values = _connectionOptions.Values,
            Input = new Pipe(setting.InputOptions),
            Output = new Pipe(setting.OutputOptions),
        };

        return Task.FromResult<IConnection>(new TcpPipeIocpConnection(
            socket: socket,
            options: connectionOptions,
            socketSenderPool: setting.SocketSenderPool,
            socketScheduler: setting.SocketSenderPool.Scheduler));
    }
}