using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel.Kestrel
{
    public class TransportPipeChannel<TPackageInfo>
        : ChannelBase<TPackageInfo>
            , IPipeChannel
    {
        private Task _readsTask;
        private bool _isDetaching;

        private readonly CancellationTokenSource _cts;
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        private readonly ILogger _logger;
        private readonly PipeReader _reader;
        private readonly PipeWriter _writer;
        private readonly ChannelOptions _options;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly DefaultObjectPipe<TPackageInfo> _packagePipe;

        public TransportPipeChannel(
            PipeReader reader,
            PipeWriter writer,
            EndPoint localEndPoint,
            EndPoint remoteEndPoint,
            IPipelineFilter<TPackageInfo> pipelineFilter,
            ChannelOptions options,
            CancellationToken connectionToken = default)
        {
            _options = options;
            _logger = options.Logger;
            _reader = reader;
            _writer = writer;
            _pipelineFilter = pipelineFilter;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(connectionToken);
            _packagePipe = options.ReadAsDemand
                ? new DefaultObjectPipeWithSupplyControl<TPackageInfo>()
                : new DefaultObjectPipe<TPackageInfo>();

            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
        }

        public TransportPipeChannel(
            IDuplexPipe transport,
            EndPoint localEndPoint,
            EndPoint remoteEndPoint,
            IPipelineFilter<TPackageInfo> pipelineFilter,
            ChannelOptions options,
            CancellationToken connectionToken = default)
            : this(transport.Input,
                transport.Output,
                localEndPoint,
                remoteEndPoint,
                pipelineFilter,
                options,
                connectionToken)
        {
        }

        #region public

        public override void Start()
        {
            _readsTask = ReadPipeAsync(_reader);
            WaitHandleClosing();
        }

        public override async IAsyncEnumerable<TPackageInfo> RunAsync()
        {
            if (_readsTask == null)
                throw new Exception("The channel has not been started yet.");

            while (true)
            {
                var package = await _packagePipe.ReadAsync();

                if (package == null)
                    yield break;

                yield return package;
            }
        }

        public override async ValueTask CloseAsync(CloseReason closeReason)
        {
            CloseReason = closeReason;
            Cancel();
            await HandleClosing().ConfigureAwait(false);
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            try
            {
                await _sendLock.WaitAsync();
                UpdateLastActiveTime();
                WriteBuffer(_writer, buffer);
                await _writer.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            try
            {
                await _sendLock.WaitAsync();
                UpdateLastActiveTime();
                WritePackageWithEncoder(_writer, packageEncoder, package);
                await _writer.FlushAsync();
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
                await _sendLock.WaitAsync();
                UpdateLastActiveTime();
                write(_writer);
                await _writer.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public override async ValueTask DetachAsync()
        {
            _isDetaching = true;
            Cancel();
            await HandleClosing().ConfigureAwait(false);
            _isDetaching = false;
        }

        #endregion

        #region protected

        protected virtual async void Close()
        {
            await _reader.CompleteAsync();
            await _writer.CompleteAsync();
        }

        protected void Cancel()
        {
            _cts.Cancel();
        }

        #endregion

        #region private

        private async void WaitHandleClosing()
        {
            await HandleClosing();
        }

        private async Task HandleClosing()
        {
            var readTask = _readsTask;

            if (readTask == null)
                return;

            try
            {
                await readTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                OnError("Unhandled exception in the method PipeChannel.Run.", e);
            }

            if (Interlocked.CompareExchange(ref _readsTask, null, readTask) != readTask)
                return;

            if (_isDetaching || IsClosed)
                return;
            
            try
            {
                Close();
                OnClosed();
            }
            catch (Exception exc)
            {
                if (!IsSocketIgnorableException(exc))
                    OnError("Unhandled exception in the method PipeChannel.Close.", exc);
            }
        }

        private static bool IsSocketIgnorableException(Exception e)
        {
            if (IsIgnorableException(e))
                return true;

            if (e is SocketException se && se.IsIgnorableSocketException())
                return true;

            return false;
        }

        private static bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException || e is NullReferenceException || e is OperationCanceledException)
                return true;

            if (e.InnerException != null)
                return IsIgnorableException(e.InnerException);

            return false;
        }

        private void ThrowChannelClosed()
        {
            if (IsClosed)
                throw new Exception("Channel is closed now, send is not allowed.");
        }

        private void UpdateLastActiveTime()
        {
            LastActiveTime = DateTimeOffset.Now;
        }

        private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
        {
            ThrowChannelClosed();
            writer.Write(buffer.Span);
        }

        private void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer,
            IPackageEncoder<TPackage> packageEncoder,
            TPackage package)
        {
            ThrowChannelClosed();
            packageEncoder.Encode(writer, package);
        }

        private async Task ReadPipeAsync(PipeReader reader)
        {
            var cancellationToken = _cts.Token;
            var supplyController = _packagePipe as ISupplyController;

            if (supplyController != null)
                cancellationToken.Register(() => supplyController.SupplyEnd());

            while (true)
            {
                if (supplyController != null)
                {
                    await supplyController.SupplyRequired().ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                        break;
                }

                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    if (!IsSocketIgnorableException(e))
                    {
                        OnError("Failed to read from the pipe", e);

                        CloseReason ??= _cts.IsCancellationRequested
                            ? SuperSocket.Channel.CloseReason.RemoteClosing
                            : SuperSocket.Channel.CloseReason.SocketError;
                    }
                    else if (!CloseReason.HasValue)
                    {
                        CloseReason = SuperSocket.Channel.CloseReason.Unknown;
                    }

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
                    if (!buffer.IsEmpty)
                    {
                        if (!ReaderBuffer(ref buffer, out consumed, out examined))
                            break;
                    }

                    if (completed)
                    {
                        CloseReason = SuperSocket.Channel.CloseReason.RemoteClosing;
                        break;
                    }

                    UpdateLastActiveTime();
                }
                catch (Exception e)
                {
                    OnError("Protocol error", e);
                    // close the connection if get a protocol error
                    CloseReason = SuperSocket.Channel.CloseReason.ProtocolError;
                    break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            if (_isDetaching || cancellationToken.IsCancellationRequested)
            {
                WriteEofPackage();
                return;
            }

            Close();

            WriteEofPackage();
        }

        private void WriteEofPackage()
        {
            _packagePipe.Write(default);
        }

        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer,
            out SequencePosition consumed,
            out SequencePosition examined)
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

        private void OnError(string message, Exception e = null)
        {
            if (e != null)
                _logger?.LogError(e, message);
            else
                _logger?.LogError(message);
        }

        #endregion

        Pipe IPipeChannel.In => throw new NotSupportedException();

        Pipe IPipeChannel.Out => throw new NotSupportedException();

        IPipelineFilter IPipeChannel.PipelineFilter => _pipelineFilter;
    }
}