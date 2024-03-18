using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    public abstract partial class PipeConnectionBase<TPackageInfo> : ConnectionBase<TPackageInfo>, IConnection<TPackageInfo>, IConnection, IPipeConnection
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        private bool _started = false;

        private CancellationTokenSource _cts = new CancellationTokenSource();

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

        protected internal IObjectPipe<TPackageInfo> PackagePipe { get; }

        protected ILogger Logger { get; }

        protected ConnectionOptions Options { get; }

        private Task _pipeTask;

        private bool _isDetaching = false;

        protected PipeConnectionBase(PipeReader inputReader, PipeWriter outputWriter, IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
        {
            _pipelineFilter = pipelineFilter;

            if (!options.ReadAsDemand)
                PackagePipe = new DefaultObjectPipe<TPackageInfo>();
            else
                PackagePipe = new DefaultObjectPipeWithSupplyControl<TPackageInfo>();

            Options = options;
            Logger = options.Logger;
            InputReader = inputReader;
            OutputWriter = outputWriter;
        }

        public override void Start()
        {
            _pipeTask = StartTask();
            _started = true;
            WaitHandleClosing();
        }

        protected virtual Task StartTask()
        {
            return ProcessReads(_cts.Token);
        }

        private async void WaitHandleClosing()
        {
            await HandleClosing().ConfigureAwait(false);
        }

        public async override IAsyncEnumerable<TPackageInfo> RunAsync()
        { 
            if (!_started)
                throw new Exception("The connection has not been started yet.");

            while (!_cts.IsCancellationRequested)
            {
                var package = await PackagePipe.ReadAsync().ConfigureAwait(false);

                if (package == null)
                {
                    yield break;
                }

                yield return package;
            }
            
            //How do empty a pipe?
        }

        private async ValueTask HandleClosing()
        {
            try
            {
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

        protected virtual async Task ProcessReads(CancellationToken cancellationToken)
        {
            await StartInputPipeTask(cancellationToken);
        }

        protected virtual Task StartInputPipeTask(CancellationToken cancellationToken)
        {
            return ReadPipeAsync(InputReader, cancellationToken);
        }

        private void CheckConnectionOpen()
        {
            if (this.IsClosed)
            {
                throw new Exception("Channel is closed now, send is not allowed.");
            }
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            try
            {
                await SendLock.WaitAsync().ConfigureAwait(false);
                WriteBuffer(OutputWriter, buffer);
                await OutputWriter.FlushAsync().ConfigureAwait(false);
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

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            try
            {
                await SendLock.WaitAsync().ConfigureAwait(false);
                WritePackageWithEncoder<TPackage>(OutputWriter, packageEncoder, package);
                await OutputWriter.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                SendLock.Release();
            }
        }

        public override async ValueTask SendAsync(Action<PipeWriter> write)
        {
            try
            {
                await SendLock.WaitAsync().ConfigureAwait(false);
                write(OutputWriter);
                await OutputWriter.FlushAsync().ConfigureAwait(false);
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

        protected internal ArraySegment<T> GetArrayByMemory<T>(ReadOnlyMemory<T> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }

        protected async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
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
            PackagePipe.Write(default);
        }

        private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var bytesConsumedTotal = 0L;

            var maxPackageLength = Options.MaxPackageLength;

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
                    currentPipelineFilter.Reset();
                }
                else
                {
                    // reset the pipeline filter after we parse one full package
                    currentPipelineFilter.Reset();
                    PackagePipe.Write(packageInfo);
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
