using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    public abstract partial class PipeConnectionBase : ConnectionBase, IConnection, IPipeConnection
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private IPipelineFilter _pipelineFilter;

        private IObjectPipe _packagePipe;

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

        private Task _pipeTask;

        private bool _isDetaching = false;

        protected PipeConnectionBase(PipeReader inputReader, PipeWriter outputWriter, ConnectionOptions options)
        {
            Options = options;
            Logger = options.Logger;
            InputReader = inputReader;
            OutputWriter = outputWriter;
            ConnectionToken = _cts.Token;
        }

        protected virtual Task StartTask<TPackageInfo>(IObjectPipe<TPackageInfo> packagePipe)
        {
            return StartInputPipeTask(packagePipe, _cts.Token);
        }

        protected void UpdateLastActiveTime()
        {
            LastActiveTime = DateTimeOffset.Now;
        }

        public async override IAsyncEnumerable<TPackageInfo> RunAsync<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter)
        {
            var packagePipe = !Options.ReadAsDemand
                ? new DefaultObjectPipe<TPackageInfo>()
                : new DefaultObjectPipeWithSupplyControl<TPackageInfo>();

            _packagePipe = packagePipe;
            _pipelineFilter = pipelineFilter;

            _pipeTask = StartTask(packagePipe);

            _ = HandleClosing();

            while (!_cts.IsCancellationRequested)
            {
                var package = await packagePipe.ReadAsync().ConfigureAwait(false);

            e
            }

            //How do empty a pipe?
        }

        private async ValueTask HandleClosing()
        {
            try
            {
                if (_pipeTask != null)
                    await _pipeTask.ConfigureAwait(false);
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
                            OnError("Unhandled exception in the method PipeChannel.Close.", exc);
                    }
                }
            }
        }

        protected abstract void Close();

        public override async ValueTask CloseAsync(CloseReason closeReason)
        {
            CloseReason = closeReason;
            Cancel();
            await HandleClosing().ConfigureAwait(false);
        }

        protected void Cancel()
        {
            _cts.Cancel();
        }

        protected virtual bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException || e is NullReferenceException)
                return true;

            if (e.InnerException != null)
                return IsIgnorableException(e.InnerException);

            return false;
        }

        protected virtual Task StartInputPipeTask<TPackageInfo>(IObjectPipe<TPackageInfo> packagePipe, CancellationToken cancellationToken)
        {
            return ReadPipeAsync(InputReader, packagePipe, cancellationToken);
        }

        private void CheckConnectionOpen()
        {
            if (this.IsClosed)
            {
                throw new Exception("Channel is closed now, send is not allowed.");
            }
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            try
            {
                await SendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                WriteBuffer(OutputWriter, buffer);
                await OutputWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                SendLock.Release();
            }
        }

        private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
        {
            CheckConnectionOpen();
            writer.Write(buffer.Span);
        }

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken = default)
        {
            try
            {
                await SendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                WritePackageWithEncoder<TPackage>(OutputWriter, packageEncoder, package);
                await OutputWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                SendLock.Release();
            }
        }

        public override async ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken)
        {
            try
            {
                await SendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                write(OutputWriter);
                await OutputWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                SendLock.Release();
            }
        }

        protected void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            CheckConnectionOpen();
            packageEncoder.Encode(writer, package);
        }

        protected virtual void OnInputPipeRead(ReadResult result)
        {
        }

        protected async Task ReadPipeAsync<TPackageInfo>(PipeReader reader, IObjectPipe<TPackageInfo> packagePipe, CancellationToken cancellationToken)
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

                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                if (result.IsCanceled)
                {
                    break;
                }

                var completed = result.IsCompleted;

                try
                {
                    if (buffer.Length > 0)
                    {
                        var needReadMore = ReaderBuffer(ref buffer, pipelineFilter, packagePipe, out consumed, out examined, out var currentPipelineFilter);

                        if (currentPipelineFilter != null)
                        {
                            pipelineFilter = currentPipelineFilter;
                        }

                        if (!needReadMore)
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
                    CloseReason = Connection.CloseReason.ProtocolError;
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

        protected void WriteEOFPackage()
        {
            _packagePipe.WirteEOF();
        }

        private bool ReaderBuffer<TPackageInfo>(ref ReadOnlySequence<byte> buffer, IPipelineFilter<TPackageInfo> pipelineFilter, IObjectPipe<TPackageInfo> packagePipe, out SequencePosition consumed, out SequencePosition examined, out IPipelineFilter<TPackageInfo> currentPipelineFilter)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            var maxPackageLength = Options.MaxPackageLength;

            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                var prevPipelineFilter = pipelineFilter;
                var filterSwitched = false;

                var packageInfo = pipelineFilter.Filter(ref seqReader);

                var nextFilter = pipelineFilter.NextFilter;

                if (nextFilter != null)
                {
                    nextFilter.Context = pipelineFilter.Context; // pass through the context
                    _pipelineFilter = pipelineFilter = nextFilter;
                    filterSwitched = true;
                }

                currentPipelineFilter = pipelineFilter;

                var bytesConsumed = seqReader.Consumed;
                bytesConsumedTotal += bytesConsumed;

                var len = bytesConsumed;

                // nothing has been consumed, need more data
                if (len == 0)
                    len = seqReader.Length;

                if (maxPackageLength > 0 && len > maxPackageLength)
                {
                    OnError($"Package cannot be larger than {maxPackageLength}.");
                    CloseReason = Connection.CloseReason.ProtocolError;
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
                    prevPipelineFilter.Reset();
                }
                else
                {
                    // reset the pipeline filter after we parse one full package
                    prevPipelineFilter.Reset();
                    packagePipe.Write(packageInfo);
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

        public override async ValueTask DetachAsync()
        {
            _isDetaching = true;
            Cancel();
            await HandleClosing().ConfigureAwait(false);
            _isDetaching = false;
        }

        protected void OnError(string message, Exception e = null)
        {
            if (e != null)
                Logger?.LogError(e, message);
            else
                Logger?.LogError(message);
        }
    }
}
