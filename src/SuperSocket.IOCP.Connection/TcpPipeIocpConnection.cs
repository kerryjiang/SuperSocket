using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using SuperSocket.Connection;
using SuperSocket.IOCP.Connection;

namespace SwatchSocket.IOCP.Connection;

public class TcpPipeIocpConnection : TcpPipeConnection
{
    private SocketSender? _sender;
    
    private readonly Socket _socket;
    private readonly bool _waitForData;
    private readonly SocketReceiver _receiver;
    private readonly SocketSenderPool _socketSenderPool;

    public TcpPipeIocpConnection(
        Socket socket,
        ConnectionOptions options,
        SocketSenderPool socketSenderPool,
        PipeScheduler? socketScheduler = default,
        bool waitForData = true) : base(socket, options)
    {
        socketScheduler ??= PipeScheduler.ThreadPool;

        _socket = socket;
        _waitForData = waitForData;
        _socketSenderPool = socketSenderPool;
        _receiver = new SocketReceiver(socketScheduler);
    }

    protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        _sender = _socketSenderPool.Rent();

        var transferResult = await _sender.SendAsync(_socket, buffer).ConfigureAwait(false);

        if (transferResult.HasError)
        {
            if (IsConnectionResetError(transferResult.SocketError.SocketErrorCode))
                throw transferResult.SocketError;

            if (IsConnectionAbortError(transferResult.SocketError.SocketErrorCode))
                throw transferResult.SocketError;
        }

        _socketSenderPool.Return(_sender);

        _sender = null;

        return transferResult.BytesTransferred;
    }

    protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory,
        CancellationToken cancellationToken)
    {
        if (_waitForData)
        {
            // Wait for data before allocating a buffer.
            var waitForDataResult = await _receiver.WaitForDataAsync(_socket).ConfigureAwait(false);

            if (waitForDataResult.HasError)
                throw waitForDataResult.SocketError;
        }

        var receiveResult = await _receiver.ReceiveAsync(_socket, memory).ConfigureAwait(false);

        if (receiveResult.HasError)
            throw receiveResult.SocketError;

        return receiveResult.BytesTransferred;
    }

    private static bool IsConnectionResetError(SocketError errorCode)
    {
        return errorCode == SocketError.ConnectionReset ||
               errorCode == SocketError.Shutdown ||
               (errorCode == SocketError.ConnectionAborted && OperatingSystem.IsWindows());
    }

    private static bool IsConnectionAbortError(SocketError errorCode)
    {
        // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
        return errorCode == SocketError.OperationAborted ||
               errorCode == SocketError.Interrupted ||
               (errorCode == SocketError.InvalidArgument && !OperatingSystem.IsWindows());
    }
}