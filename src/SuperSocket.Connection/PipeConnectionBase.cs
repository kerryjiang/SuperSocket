using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.ProtoBase.ProxyProtocol;
using System.Runtime.CompilerServices;

namespace SuperSocket.Connection
{
    public abstract partial class PipeConnectionBase : ConnectionBase, IConnection, IPipeConnection
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private IPipelineFilter _pipelineFilter;

        protected SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);

        protected PipeWriter OutputWriter { get; }

        PipeWriter IPipeConnection.OutputWriter
        {
            get { return OutputWriter; }
        }

        protected PipeReader InputReader { get; }

        PipeReader IPipeConnection.InputReader
        {
            get { return InputReader; }
        }

        IPipelineFilter IPipeConnection.PipelineFilter
        {
            get { return _pipelineFilter; }
        }

        protected ILogger Logger { get; }

        protected ConnectionOptions Options { get; }

        private Task _connectionTask;

        private bool _isDetaching = false;

        protected PipeConnectionBase(PipeReader inputReader, PipeWriter outputWriter, ConnectionOptions options)
        {
            Options = options;
            Logger = options.Logger;
            InputReader = inputReader;
            OutputWriter = outputWriter;
            ConnectionToken = _cts.Token;
        }

        protected void UpdateLastActiveTime()
        {
            LastActiveTime = DateTimeOffset.Now;
        }

        protected virtual async Task GetConnectionTask(Task readTask, CancellationToken cancellationToken)
        {
            await readTask.ConfigureAwait(false);
            FireClose();
        }
        
        public async override IAsyncEnumerable<TPackageInfo> RunAsync<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter)
        {
            _pipelineFilter = pipelineFilter;

            var readTaskCompletionSource = new TaskCompletionSource();
            _connectionTask = GetConnectionTask(readTaskCompletionSource.Task, _cts.Token);

            var packagePipeEnumerator = ReadPipeAsync<TPackageInfo>(InputReader, _cts.Token).GetAsyncEnumerator(_cts.Token);

            while (true)
            {
                var read = false;

                try
                {
                    read = await packagePipeEnumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    OnError("Unhandled exception in the method PipeConnection.Run.", e);
                    break;
                }

                if (read)
                {
                    yield return packagePipeEnumerator.Current;
                    continue;
                }

                break;
            }

            readTaskCompletionSource.SetResult();
            yield break;
        }

        private void FireClose()
        {
            if (!_isDetaching && !IsClosed)
            {
                try
                {
                    Close();
                    OnClosed();
                }
                catch (Exception exc)
                {
                    if (!IsIgnorableException(exc))
                        OnError("Unhandled exception in the method PipeConnection.Close.", exc);
                }
            }
        }

        protected abstract void Close();

        public override async ValueTask CloseAsync(CloseReason closeReason)
        {
            CloseReason = closeReason;
            await CancelAsync().ConfigureAwait(false);

            if (_connectionTask is Task connectionTask)
                await connectionTask.ConfigureAwait(false);
        }

        protected async Task CancelAsync()
        {
            if (_cts.IsCancellationRequested)
                return;

            _cts.Cancel();
            await CompleteWriterAsync(OutputWriter, _isDetaching).ConfigureAwait(false);
            CancelOutputPendingRead();
        }

        protected virtual bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException || e is NullReferenceException)
                return true;

            if (e.InnerException != null)
                return IsIgnorableException(e.InnerException);

            return false;
        }

        private void CheckConnectionSendAllowed()
        {
            if (this.IsClosed)
            {
                throw new Exception("Connection is closed now, send is not allowed.");
            }

            if (_cts.IsCancellationRequested)
            {
                throw new Exception("The communication over this connection is being closed, send is not allowed.");
            }
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var sendLockAcquired = false;

            try
            {
                await SendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                sendLockAcquired = true;
                WriteBuffer(OutputWriter, buffer);
                await OutputWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (sendLockAcquired)
                    SendLock.Release();
            }
        }

        private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
        {
            CheckConnectionSendAllowed();
            writer.Write(buffer.Span);
        }

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken = default)
        {
            var sendLockAcquired = false;

            try
            {
                await SendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                sendLockAcquired = true;
                WritePackageWithEncoder<TPackage>(OutputWriter, packageEncoder, package);
                await OutputWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (sendLockAcquired)
                    SendLock.Release();
            }
        }

        public override async ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken)
        {
            CheckConnectionSendAllowed();
            
            var sendLockAcquired = false;

            try
            {
                await SendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                sendLockAcquired = true;
                write(OutputWriter);
                await OutputWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (sendLockAcquired)
                    SendLock.Release();
            }
        }

        protected void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            CheckConnectionSendAllowed();
            packageEncoder.Encode(writer, package);
        }

        protected virtual void OnInputPipeRead(ReadResult result)
        {
        }

        protected async IAsyncEnumerable<TPackageInfo> ReadPipeAsync<TPackageInfo>(PipeReader reader, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var pipelineFilter = _pipelineFilter as IPipelineFilter<TPackageInfo>;

            while (!cancellationToken.IsCancellationRequested)
            {
                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    OnInputPipeRead(result);
                }
                catch (Exception e)
                {
                    if (!IsIgnorableException(e) && !(e is OperationCanceledException))
                        OnError("Failed to read from the pipe", e);

                    break;
                }

                var buffer = result.Buffer;

                SequencePosition consumed = buffer.End;

                var completedOrCancelled = result.IsCompleted || result.IsCanceled;

                if (buffer.Length > 0)
                {
                    BufferFilterResult<TPackageInfo> lastFilterResult = default;

                    foreach (var bufferFilterResult in ReadBuffer(buffer, pipelineFilter))
                    {
                        lastFilterResult = bufferFilterResult;

                        if (bufferFilterResult.Package != null)
                        {
                            yield return bufferFilterResult.Package;
                        }

                        if (bufferFilterResult.Exception != null)
                        {
                            OnError("Protocol error", bufferFilterResult.Exception);
                            CloseReason = Connection.CloseReason.ProtocolError;
                            Close();
                            completedOrCancelled = true;
                            break;
                        }
                    }

                    pipelineFilter = _pipelineFilter as IPipelineFilter<TPackageInfo>;

                    if (lastFilterResult.Consumed > 0)
                    {
                        consumed = buffer.GetPosition(lastFilterResult.Consumed);
                        reader.AdvanceTo(consumed, buffer.End);
                    }
                    else
                    {
                        reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }                

                if (completedOrCancelled)
                {
                    break;
                }
            }

            await CompleteReaderAsync(reader, _isDetaching).ConfigureAwait(false);
            yield break;
        }

        private IEnumerable<BufferFilterResult<TPackageInfo>> ReadBuffer<TPackageInfo>(ReadOnlySequence<byte> buffer, IPipelineFilter<TPackageInfo> pipelineFilter)
        {
            var bytesConsumedTotal = 0L;
            var maxPackageLength = Options.MaxPackageLength;

            while (true)
            {
                var prevPipelineFilter = pipelineFilter;
                var filterSwitched = false;

                TPackageInfo packageInfo = default;

                Exception exception = null;

                var readerConsumed = 0L;
                var readerEnd = false;

                try
                {
                    var seqReader = new SequenceReader<byte>(buffer);
                    packageInfo = pipelineFilter.Filter(ref seqReader);
                    readerConsumed = seqReader.Consumed;
                    readerEnd = seqReader.End;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                if (exception != null)
                {
                    yield return new BufferFilterResult<TPackageInfo>(exception);
                    yield break;
                }

                var nextFilter = pipelineFilter.NextFilter;

                if (nextFilter != null)
                {
                    // ProxyProtocolPipelineFilter always is the first filter and its next filter is the actual first filter.
                    if (bytesConsumedTotal == 0 && pipelineFilter is IProxyProtocolPipelineFilter proxyProtocolPipelineFilter)
                    {
                        ProxyInfo = proxyProtocolPipelineFilter.ProxyInfo;
                    }

                    nextFilter.Context = pipelineFilter.Context; // pass through the context
                    _pipelineFilter = pipelineFilter = nextFilter;
                    filterSwitched = true;
                }

                bytesConsumedTotal += readerConsumed;

                var len = readerConsumed;

                // nothing has been consumed, need more data
                if (len == 0)
                    len = buffer.Length;

                if (maxPackageLength > 0 && len > maxPackageLength)
                {
                    yield return new BufferFilterResult<TPackageInfo>(new Exception($"Package cannot be larger than {maxPackageLength}."));
                    yield break;
                }

                if (packageInfo != null || filterSwitched)
                {
                    // We should reset the previous pipeline filter after switch or parse one full package.
                    prevPipelineFilter.Reset();
                }

                var needReadMore = readerEnd // has consumed all the data in the buffer
                    || (packageInfo == null // not parsed a full package yet
                        && !filterSwitched);// and not switch to another filter

                if (!readerEnd && readerConsumed > 0)
                    buffer = buffer.Slice(readerConsumed);

                if (packageInfo != null || needReadMore)
                {
                    yield return new BufferFilterResult<TPackageInfo>(packageInfo, bytesConsumedTotal);

                    if (needReadMore)
                    {
                        yield break;
                    }
                }
            }
        }

        public override async ValueTask DetachAsync()
        {
            _isDetaching = true;
            await CancelAsync().ConfigureAwait(false);
            await _connectionTask.ConfigureAwait(false);
            _isDetaching = false;
        }

        protected void OnError(string message, Exception e = null)
        {
            if (e != null)
                Logger?.LogError(e, message);
            else
                Logger?.LogError(message);
        }
        
        protected virtual async ValueTask CompleteReaderAsync(PipeReader reader, bool isDetaching)
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }

        protected virtual async ValueTask CompleteWriterAsync(PipeWriter writer, bool isDetaching)
        {
            await writer.CompleteAsync().ConfigureAwait(false);
        }

        protected virtual void CancelOutputPendingRead()
        {
        }

        internal struct BufferFilterResult<TPackageInfo>
        {
            public Exception Exception { get; set; }

            public TPackageInfo Package { get; set; }

            public long Consumed { get; set; }

            public BufferFilterResult(TPackageInfo packageInfo)
                : this(packageInfo, default)
            {
            }

            public BufferFilterResult(Exception exception)
            {
                Exception = exception;
            }

            public BufferFilterResult(TPackageInfo packageInfo, long consumed)
            {
                Package = packageInfo;
                Consumed = consumed;
            }
        }
    }
}
