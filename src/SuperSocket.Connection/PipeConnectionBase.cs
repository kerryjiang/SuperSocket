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
    /// <summary>
    /// Provides a base class for pipe-based connections, implementing common connection functionality.
    /// </summary>
    public abstract partial class PipeConnectionBase : ConnectionBase, IConnection, IPipeConnection
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private IPipelineFilter _pipelineFilter;

        /// <summary>
        /// Gets the semaphore used to synchronize send operations.
        /// </summary>
        protected SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Gets the pipe writer for output data.
        /// </summary>
        protected PipeWriter OutputWriter { get; }

        PipeWriter IPipeConnection.OutputWriter
        {
            get { return OutputWriter; }
        }

        /// <summary>
        /// Gets the pipe reader for input data.
        /// </summary>
        protected PipeReader InputReader { get; }

        PipeReader IPipeConnection.InputReader
        {
            get { return InputReader; }
        }

        IPipelineFilter IPipeConnection.PipelineFilter
        {
            get { return _pipelineFilter; }
        }

        /// <summary>
        /// Gets the logger used for logging connection events.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the connection options.
        /// </summary>
        protected ConnectionOptions Options { get; }

        private Task _connectionTask;

        private bool _isDetaching = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeConnectionBase"/> class with the specified input and output pipes and connection options.
        /// </summary>
        /// <param name="inputReader">The pipe reader for input data.</param>
        /// <param name="outputWriter">The pipe writer for output data.</param>
        /// <param name="options">The connection options.</param>
        protected PipeConnectionBase(PipeReader inputReader, PipeWriter outputWriter, ConnectionOptions options)
        {
            Options = options;
            Logger = options.Logger;
            InputReader = inputReader;
            OutputWriter = outputWriter;
            ConnectionToken = _cts.Token;
        }

        /// <summary>
        /// Updates the last active time of the connection to the current time.
        /// </summary>
        protected void UpdateLastActiveTime()
        {
            LastActiveTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Gets a task which represents the connection.
        /// </summary>
        /// <param name="readTask">The read task.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected virtual async Task GetConnectionTask(Task readTask, CancellationToken cancellationToken)
        {
            await readTask.ConfigureAwait(false);
            FireClose();
        }
        
        /// <summary>
        /// Runs the connection asynchronously with the specified pipeline filter.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
        /// <param name="pipelineFilter">The pipeline filter to use for processing data.</param>
        /// <returns>An asynchronous enumerable of package information.</returns>
        public async override IAsyncEnumerable<TPackageInfo> RunAsync<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter)
        {
            _pipelineFilter = pipelineFilter;

            var readTaskCompletionSource = new TaskCompletionSource();

            _cts.Token.Register(() =>
            {
                readTaskCompletionSource.TrySetResult();
            });

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

            readTaskCompletionSource.TrySetResult();

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

        /// <summary>
        /// Closes the connection and releases associated resources.
        /// </summary>
        protected abstract void Close();

        /// <summary>
        /// Closes the connection asynchronously with the specified reason.
        /// </summary>
        /// <param name="closeReason">The reason for closing the connection.</param>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        public override async ValueTask CloseAsync(CloseReason closeReason)
        {
            CloseReason = closeReason;
            await CancelAsync().ConfigureAwait(false);

            if (_connectionTask is Task connectionTask)
                await connectionTask.ConfigureAwait(false);
            else // the communication over this connection has not been started yet, so fire the close event directly.
                FireClose();
        }

        /// <summary>
        /// Cancels all the operations on the connection.
        /// </summary>
        protected async Task CancelAsync()
        {
            if (_cts.IsCancellationRequested)
                return;

            _cts.Cancel();
            await CompleteWriterAsync(OutputWriter, _isDetaching).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if the specified exception is ignorable.
        /// </summary>
        /// <param name="e">The exception.</param>
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

        /// <summary>
        /// Sends data over the connection asynchronously using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
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

        /// <summary>
        /// Sends data over the connection asynchronously using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public override async ValueTask SendAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken = default)
        {
            var sendLockAcquired = false;

            try
            {
                await SendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                sendLockAcquired = true;
                await WriteBufferAsync(OutputWriter, buffer).ConfigureAwait(false);
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

        private async Task WriteBufferAsync(PipeWriter writer, ReadOnlySequence<byte> buffer)
        {
            CheckConnectionSendAllowed();
            if (buffer.IsSingleSegment)
            {
                await writer.WriteAsync(buffer.First).ConfigureAwait(false);
            }
            else
            {
                foreach (var memory in buffer)
                {
                    await writer.WriteAsync(memory).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Sends a package over the connection asynchronously using the specified encoder and package.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package to send.</typeparam>
        /// <param name="packageEncoder">The encoder to use for the package.</param>
        /// <param name="package">The package to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
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

        /// <summary>
        /// Sends data over the connection asynchronously using a custom write action.
        /// </summary>
        /// <param name="write">The action to write data to the pipe writer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
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

        /// <summary>
        /// Writes a package to the output writer using the specified encoder.
        /// </summary>
        /// <typeparam name="TPackage">The package type.</typeparam>
        /// <param name="writer">The buffer writer.</param>
        /// <param name="packageEncoder">The package encoder.</param>
        /// <param name="package">The package.</param>
        protected void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            CheckConnectionSendAllowed();
            packageEncoder.Encode(writer, package);
        }

        /// <summary>
        /// Invoked when data is read from the input pipe.
        /// </summary>
        /// <param name="result">The read result.</param>
        protected virtual void OnInputPipeRead(ReadResult result)
        {
        }

        /// <summary>
        /// Reads data from the input pipe asynchronously and processes it using the specified pipeline filter.
        /// </summary>
        /// <typeparam name="TPackageInfo">The package type.</typeparam>
        /// <param name="reader">The pipe reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
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

        /// <summary>
        /// Detaches the connection asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous detach operation.</returns>
        public override async ValueTask DetachAsync()
        {
            _isDetaching = true;
            await CancelAsync().ConfigureAwait(false);
            await _connectionTask.ConfigureAwait(false);
            _isDetaching = false;
        }

        /// <summary>
        /// Handles errors that occur during connection operations.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="e">The exception that occurred, if any.</param>
        protected void OnError(string message, Exception e = null)
        {
            if (e != null)
                Logger?.LogError(e, message);
            else
                Logger?.LogError(message);
        }
        
        /// <summary>
        /// Completes the reader asynchronously.
        /// </summary>
        /// <param name="reader">The pipe reader.</param>
        /// <param name="isDetaching">Indicates if this operation is a part of detaching action.</param>
        protected virtual async ValueTask CompleteReaderAsync(PipeReader reader, bool isDetaching)
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Completes the writer asynchronously.
        /// </summary>
        /// <param name="writer">The pipe writer.</param>
        /// <param name="isDetaching">Indicates if this operation is a part of detaching action.</param>
        protected virtual async ValueTask CompleteWriterAsync(PipeWriter writer, bool isDetaching)
        {
            await writer.CompleteAsync().ConfigureAwait(false);
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
