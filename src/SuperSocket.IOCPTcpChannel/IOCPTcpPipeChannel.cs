using SuperSocket.Channel;
using SuperSocket.Kestrel.IOCP;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.IOCPTcpChannel;

public sealed class IOCPTcpPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
{
    private Socket? _socket;
    private SocketSender? _sender;
    private readonly bool _waitForData;
    private readonly SocketReceiver _receiver;
    private readonly SocketSenderPool _socketSenderPool;

    public IOCPTcpPipeChannel(Socket socket,
                              IPipelineFilter<TPackageInfo> pipelineFilter,
                              ChannelOptions options,
                              SocketSenderPool socketSenderPool,
                              PipeScheduler? socketScheduler = default,
                              bool waitForData = true) :
        base(pipelineFilter, options)
    {
        socketScheduler ??= PipeScheduler.ThreadPool;

        _socket = socket;
        _receiver = new SocketReceiver(socketScheduler);
        _socketSenderPool = socketSenderPool;

        _waitForData = waitForData;
    }

    public override ValueTask CloseAsync(CloseReason closeReason)
    {
        return base.CloseAsync(closeReason);
    }

    /// <summary>
    /// 从socket中接受数据流然后写入memory
    /// </summary>
    /// <param name="memory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory,
                                                                  CancellationToken cancellationToken)
    {
        if (_waitForData)
        {
            // Wait for data before allocating a buffer.
            var waitForDataResult = await _receiver.WaitForDataAsync(_socket!).ConfigureAwait(false);

            if (waitForDataResult.HasError)
                throw waitForDataResult.SocketError;
        }

        var receiveResult = await _receiver.ReceiveAsync(_socket!, memory).ConfigureAwait(false);

        if (receiveResult.HasError)
            throw receiveResult.SocketError;

        return receiveResult.BytesTransferred;
    }

    /// <summary>
    /// 发送数据至socket
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer,
                                                            CancellationToken cancellationToken)
    {
        _sender = _socketSenderPool.Rent();

        var transferResult = await _sender.SendAsync(_socket!, buffer).ConfigureAwait(false);

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

    /// <summary>
    /// 从pipeline中读取数据然后发送至socket
    /// </summary>
    /// <returns></returns>
    protected override async Task ProcessSends()
    {
        var cts = Cts;
        var output = Out.Reader;

        bool completed;
        ReadResult result;
        SequencePosition end;
        ReadOnlySequence<byte> buffer;

        while (true)
        {
            result = await output.ReadAsync().ConfigureAwait(false);

            completed = result.IsCompleted;

            buffer = result.Buffer;
            end = buffer.End;

            if (!buffer.IsEmpty)
            {
                try
                {
                    await SendOverIOAsync(buffer, cts.Token).ConfigureAwait(false);

                    LastActiveTime = DateTimeOffset.Now;
                }
                catch (Exception e)
                {
                    cts?.Cancel(false);

                    if (!IsIgnorableException(e))
                        OnError("Exception happened in SendAsync", e);

                    break;
                }
            }

            output.AdvanceTo(end);

            if (completed)
                break;
        }

        output.Complete();
    }

    /// <summary>
    /// 从socket中读取数据流然后写入 pipeline
    /// </summary>
    /// <param name="writer"></param>
    /// <returns></returns>
    protected override async Task FillPipeAsync(PipeWriter writer)
    {
        var options = Options;
        var cts = Cts;

        Memory<byte> memory;
        FlushResult result;
        int bufferSize, maxPackageLength, bytesRead;
        SocketOperationResult receiveResult;

        while (!cts.IsCancellationRequested)
        {
            try
            {
                bufferSize = options.ReceiveBufferSize;
                maxPackageLength = options.MaxPackageLength;

                if (bufferSize <= 0)
                    bufferSize = 1024 * 4; //4k

                memory = writer.GetMemory(bufferSize);

                if (_waitForData)
                {
                    // Wait for data before allocating a buffer.
                    var waitForDataResult = await _receiver.WaitForDataAsync(_socket!).ConfigureAwait(false);

                    if (waitForDataResult.HasError)
                        throw waitForDataResult.SocketError;
                }

                receiveResult = await _receiver.ReceiveAsync(_socket!, memory).ConfigureAwait(false);

                if (receiveResult.HasError)
                    throw receiveResult.SocketError;

                bytesRead = receiveResult.BytesTransferred;

                //bytesRead = await FillPipeWithDataAsync(memory, _cts.Token);//不能使用这个封装方法 否则内存占用是封装的两倍

                if (bytesRead == 0)
                {
                    if (!CloseReason.HasValue)
                        CloseReason = Channel.CloseReason.RemoteClosing;

                    break;
                }

                LastActiveTime = DateTimeOffset.Now;

                // Tell the PipeWriter how much was read
                writer.Advance(bytesRead);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                {
                    if (e is not OperationCanceledException)
                        OnError("Exception happened in ReceiveAsync", e);

                    if (!CloseReason.HasValue)
                    {
                        CloseReason = cts.IsCancellationRequested
                            ? Channel.CloseReason.LocalClosing : Channel.CloseReason.SocketError;
                    }
                }
                else if (!CloseReason.HasValue)
                {
                    CloseReason = Channel.CloseReason.RemoteClosing;
                }

                break;
            }

            // Make the data available to the PipeReader
            result = await writer.FlushAsync().ConfigureAwait(false);

            if (result.IsCompleted)
                break;
        }

        // Signal to the reader that we're done writing
        await writer.CompleteAsync().ConfigureAwait(false);
        // And don't allow writing data to outgoing pipeline
        await Out.Writer.CompleteAsync().ConfigureAwait(false);
    }

    protected override void OnClosed()
    {
        _socket = null;
        _sender?.Dispose();
        _receiver.Dispose();
        base.OnClosed();
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