using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.Kestrel.Internal;
using SuperSocket.Kestrel.IOCP;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Kestrel.Channel;

public sealed class KestrelIOCPChannel<TPackageInfo> :
    ChannelBase<TPackageInfo>,
    IChannel<TPackageInfo>,
    IChannel,
    IPipeChannel
{
    private Task _readsTask;
    private Task _sendsTask;
    private Socket _socket;
    private SocketSender _sender;
    private IPipelineFilter<TPackageInfo> _pipelineFilter;

    private readonly Pipe _in;
    private readonly Pipe _out;
    private readonly ILogger _logger;
    private readonly bool _waitForData;
    private readonly ChannelOptions _options;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();
    private readonly SocketReceiver _receiver;
    private readonly SocketSenderPool _socketSenderPool;
    private readonly KestrelObjectPipe<TPackageInfo> _packagePipe = new();

    Pipe IPipeChannel.Out => _out;

    Pipe IPipeChannel.In => _in;

    IPipelineFilter IPipeChannel.PipelineFilter => _pipelineFilter;

    public KestrelIOCPChannel(Socket socket,
                              IPipelineFilter<TPackageInfo> pipelineFilter,
                              ChannelOptions options,
                              SocketSenderPool socketSenderPool,
                              PipeScheduler socketScheduler = default,
                              bool waitForData = true)
    {
        _pipelineFilter = pipelineFilter;

        _socket = socket;
        _options = options;
        _waitForData = waitForData;
        _logger = options.Logger;
        _socketSenderPool = socketSenderPool;
        _receiver = new SocketReceiver(socketScheduler);
        _out = options.Out ?? new Pipe();
        _in = options.In ?? new Pipe();
        RemoteEndPoint = socket.RemoteEndPoint;
        LocalEndPoint = socket.LocalEndPoint;
    }

    #region public

    public override void Start()
    {
        _readsTask = ProcessReadsAsync();
        _sendsTask = ProcessSendsAsync();
        WaitHandleClosing();
    }

    public async override IAsyncEnumerable<TPackageInfo> RunAsync()
    {
        if (_readsTask == null || _sendsTask == null)
            throw new Exception("The channel has not been started yet.");

        while (true)
        {
            var package = await _packagePipe.ReadAsync().ConfigureAwait(false);

            if (package == null)
                yield break;

            yield return package;
        }
    }

    public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
    {
        try
        {
            await _sendLock.WaitAsync().ConfigureAwait(false);
            var writer = _out.Writer;
            WriteBuffer(writer, buffer);
            await writer.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public override ValueTask DetachAsync()
    {
        throw new NotImplementedException();
    }

    private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
    {
        ThrowIfChannelClosed();
        writer.Write(buffer.Span);
    }

    public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
    {
        try
        {
            await _sendLock.WaitAsync().ConfigureAwait(false);
            var writer = _out.Writer;
            WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);
            await writer.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public override async ValueTask SendAsync(Action<PipeWriter> write)
    {
        try
        {
            await _sendLock.WaitAsync().ConfigureAwait(false);
            var writer = _out.Writer;
            write(writer);
            await writer.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public override ValueTask CloseAsync(CloseReason closeReason)
    {
        CloseReason = closeReason;
        _cts.Cancel();
        Close();

        return ValueTask.CompletedTask;
    }

    #endregion

    #region private

    private async void WaitHandleClosing()
    {
        await HandleClosing().ConfigureAwait(false);
    }

    private async ValueTask HandleClosing()
    {
        try
        {
            await Task.WhenAll(_readsTask, _sendsTask).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            OnError("Unhandled exception in the method PipeChannel.Run.", e);
        }
        finally
        {
            if (!IsClosed)
            {
                try
                {
                    Close();
                    OnClosed();
                }
                catch (Exception exc)
                {
                    if (!IsIgnorableException(exc))
                        OnError("Unhandled exception in the method PipeChannel.Close.", exc);
                }
            }
        }
    }

    private void Close()
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

    /// <summary>
    /// 从socket中读取数据流然后写入 pipeline
    /// </summary>
    /// <param name="writer"></param>
    /// <returns></returns>
    private async Task FillPipeAsync(PipeWriter writer)
    {
        var options = _options;
        var cts = _cts;

        while (!cts.IsCancellationRequested)
        {
            try
            {
                var bufferSize = options.ReceiveBufferSize;
                var maxPackageLength = options.MaxPackageLength;

                if (bufferSize <= 0)
                    bufferSize = 1024 * 4; //4k

                var memory = writer.GetMemory(bufferSize);

                if (_waitForData)
                {
                    // Wait for data before allocating a buffer.
                    var waitForDataResult = await _receiver.WaitForDataAsync(_socket);

                    if (waitForDataResult.HasError)
                        throw waitForDataResult.SocketError;
                }

                var receiveResult = await _receiver.ReceiveAsync(_socket, memory);

                if (receiveResult.HasError)
                    throw receiveResult.SocketError;

                var bytesRead = receiveResult.BytesTransferred;

                if (bytesRead == 0)
                {
                    if (!CloseReason.HasValue)
                        CloseReason = SuperSocket.Channel.CloseReason.RemoteClosing;

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
                            ? SuperSocket.Channel.CloseReason.LocalClosing : SuperSocket.Channel.CloseReason.SocketError;
                    }
                }
                else if (!CloseReason.HasValue)
                {
                    CloseReason = SuperSocket.Channel.CloseReason.RemoteClosing;
                }

                break;
            }

            // Make the data available to the PipeReader
            var result = await writer.FlushAsync().ConfigureAwait(false);

            if (result.IsCompleted)
                break;
        }

        // Signal to the reader that we're done writing
        await writer.CompleteAsync().ConfigureAwait(false);
        // And don't allow writing data to outgoing pipeline
        await _out.Writer.CompleteAsync().ConfigureAwait(false);
    }

    private async Task ProcessReadsAsync()
    {
        var pipe = _in;

        Task writing = FillPipeAsync(pipe.Writer);
        Task reading = ReadPipeAsync(pipe.Reader);

        await Task.WhenAll(reading, writing).ConfigureAwait(false);
    }

    /// <summary>
    /// 从输出pipeline中读取数据流然后写入socket
    /// </summary>
    /// <returns></returns>
    private async Task ProcessSendsAsync()
    {
        var output = _out.Reader;
        var cts = _cts;

        while (true)
        {
            var result = await output.ReadAsync();

            var completed = result.IsCompleted;

            var buffer = result.Buffer;
            var end = buffer.End;

            if (!buffer.IsEmpty)
            {
                try
                {
                    _sender = _socketSenderPool.Rent();

                    var transferResult = await _sender.SendAsync(_socket, buffer);

                    if (transferResult.HasError)
                        throw transferResult.SocketError;

                    _socketSenderPool.Return(_sender);

                    _sender = null;

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

    private void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
    {
        ThrowIfChannelClosed();
        packageEncoder.Encode(writer, package);
    }

    /// <summary>
    /// 从pipeline读取数据流然后进行包处理
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private async Task ReadPipeAsync(PipeReader reader)
    {
        var cts = _cts;
        ReadResult result;

        while (!cts.IsCancellationRequested)
        {
            try
            {
                result = await reader.ReadAsync(cts.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e) && e is not OperationCanceledException)
                    OnError("Failed to read from the pipe", e);

                break;
            }

            var buffer = result.Buffer;

            SequencePosition consumed = buffer.Start;
            SequencePosition examined = buffer.End;

            if (result.IsCanceled)
                break;

            var completed = result.IsCompleted;

            try
            {
                if (buffer.Length > 0)
                {
                    if (!ReaderBuffer(ref buffer, out consumed, out examined))
                    {
                        completed = true;
                        break;
                    }
                }

                if (completed)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                OnError("Protocol error", e);
                // close the connection if get a protocol error
                CloseReason = SuperSocket.Channel.CloseReason.ProtocolError;
                Close();
                break;
            }
            finally
            {
                reader.AdvanceTo(consumed, examined);
            }
        }

        reader.Complete();
        WriteEOFPackage();
    }

    private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
    {
        consumed = buffer.Start;
        examined = buffer.End;

        var bytesConsumedTotal = 0L;

        var maxPackageLength = _options.MaxPackageLength;

        var seqReader = new SequenceReader<byte>(buffer);

        while (true)
        {
            var currentPipelineFilter = _pipelineFilter;
            var filterSwitched = false;

            var packageInfo = currentPipelineFilter.Filter(ref seqReader);

            var nextFilter = currentPipelineFilter.NextFilter;

            if (nextFilter != null)
            {
                nextFilter.Context = currentPipelineFilter.Context; // pass through the context
                _pipelineFilter = nextFilter;
                filterSwitched = true;
            }

            var bytesConsumed = seqReader.Consumed;
            bytesConsumedTotal += bytesConsumed;

            var len = bytesConsumed;

            // nothing has been consumed, need more data
            if (len == 0)
                len = seqReader.Length;

            if (maxPackageLength > 0 && len > maxPackageLength)
            {
                OnError($"Package cannot be larger than {maxPackageLength}.");
                CloseReason = SuperSocket.Channel.CloseReason.ProtocolError;
                // close the the connection directly
                Close();
                return false;
            }

            if (packageInfo == null)
            {
                // the current pipeline filter needs more data to process
                if (!filterSwitched)
                {
                    // set consumed position and then continue to receive...
                    consumed = buffer.GetPosition(bytesConsumedTotal);
                    return true;
                }

                // we should reset the previous pipeline filter after switch
                currentPipelineFilter.Reset();
            }
            else
            {
                // reset the pipeline filter after we parse one full package
                currentPipelineFilter.Reset();
                _packagePipe.Write(packageInfo);
            }

            if (seqReader.End) // no more data
            {
                examined = consumed = buffer.End;
                return true;
            }

            if (bytesConsumed > 0)
                seqReader = new SequenceReader<byte>(seqReader.Sequence.Slice(bytesConsumed));
        }
    }

    private void WriteEOFPackage()
    {
        _packagePipe.Write(default);
    }

    private void OnError(string message, Exception e = null)
    {
        if (e != null)
            _logger?.LogError(e, message);
        else
            _logger?.LogError(message);
    }

    private void ThrowIfChannelClosed()
    {
        if (IsClosed)
            throw new Exception("Channel is closed now, send is not allowed.");
    }

    private bool IsIgnorableException(Exception e)
    {
        if (e is ObjectDisposedException or NullReferenceException or SocketException)
            return true;

        if (e.InnerException != null)
            return IsIgnorableException(e.InnerException);

        return false;
    }

    private static bool IsNormalCompletion(SocketOperationResult result)
    {
        if (!result.HasError)
            return true;

        if (IsConnectionResetError(result.SocketError.SocketErrorCode))
            return false;

        if (IsConnectionAbortError(result.SocketError.SocketErrorCode))
            return false;

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

    #endregion

    protected override void OnClosed()
    {
        _socket = null;
        _sender?.Dispose();
        _receiver.Dispose();
        base.OnClosed();
    }
}
