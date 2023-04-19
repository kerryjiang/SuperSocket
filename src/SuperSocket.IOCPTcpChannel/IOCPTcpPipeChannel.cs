using SuperSocket.Channel;
using SuperSocket.Kestrel.IOCP;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.IOCPTcpChannel;

public sealed class IOCPTcpPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
{
    private Socket? _socket;
    private SocketSender? _sender;
    private readonly SocketReceiver _receiver;
    private readonly SocketSenderPool _socketSenderPool;

    public IOCPTcpPipeChannel(Socket socket,
                              IPipelineFilter<TPackageInfo> pipelineFilter,
                              ChannelOptions options,
                              SocketSenderPool socketSenderPool,
                              PipeScheduler? socketScheduler = default) :
        base(pipelineFilter, options)
    {
        socketScheduler ??= PipeScheduler.ThreadPool;

        _socket = socket;
        _receiver = new SocketReceiver(socketScheduler);
        _socketSenderPool = socketSenderPool;

        RemoteEndPoint = socket.RemoteEndPoint;
        LocalEndPoint = socket.LocalEndPoint;
    }

    protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory,
        CancellationToken cancellationToken)
    {
        var waitForDataResult = await _receiver.WaitForDataAsync(_socket!);

        if (waitForDataResult.HasError)
            throw waitForDataResult.SocketError;

        var receiveResult = await _receiver.ReceiveAsync(_socket!, memory);

        if (waitForDataResult.HasError)
            throw waitForDataResult.SocketError;

        return receiveResult.BytesTransferred;
    }

    protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        _sender = _socketSenderPool.Rent();

        var transferResult = await _sender.SendAsync(_socket!, buffer);

        if (transferResult.HasError)
            throw transferResult.SocketError;

        // We don't return to the pool if there was an exception, and
        // we keep the _sender assigned so that we can dispose it in StartAsync.
        _socketSenderPool.Return(_sender);
        _sender = null;

        return transferResult.BytesTransferred;
    }

    protected override void Close()
    {
        var socket = _socket;

        if (socket == null)
            return;

        if (Interlocked.CompareExchange(ref _socket, null, socket) != socket)
            return;

        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        finally
        {
            socket.Close();
        }
    }

    protected override bool IsIgnorableException(Exception e)
    {
        if (base.IsIgnorableException(e))
            return true;

        if (e is SocketException se)
        {
            if (se.IsIgnorableSocketException())
                return true;
        }

        return false;
    }

    protected override void OnClosed()
    {
        _socket = null;
        _sender?.Dispose();
        _receiver.Dispose();
        base.OnClosed();
    }

    //private static bool IsNormalCompletion(SocketOperationResult result)
    //{
    //    if (!result.HasError)
    //        return true;

    //    if (IsConnectionResetError(result.SocketError.SocketErrorCode))
    //        return false;

    //    if (IsConnectionAbortError(result.SocketError.SocketErrorCode))
    //        return false;

    //    return false;
    //}

    //private static bool IsConnectionResetError(SocketError errorCode)
    //{
    //    return errorCode == SocketError.ConnectionReset ||
    //           errorCode == SocketError.Shutdown ||
    //           (errorCode == SocketError.ConnectionAborted && OperatingSystem.IsWindows());
    //}

    //private static bool IsConnectionAbortError(SocketError errorCode)
    //{
    //    // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
    //    return errorCode == SocketError.OperationAborted ||
    //           errorCode == SocketError.Interrupted ||
    //           (errorCode == SocketError.InvalidArgument && !OperatingSystem.IsWindows());
    //}
}