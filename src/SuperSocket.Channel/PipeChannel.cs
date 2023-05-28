using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;


[assembly: InternalsVisibleTo("Test")] 
namespace SuperSocket.Channel
{
    public abstract partial class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IChannel<TPackageInfo>, IChannel, IPipeChannel
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        protected SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);

        protected Pipe Out { get; }

        Pipe IPipeChannel.Out
        {
            get { return Out; }
        }

        protected Pipe In { get; }

        Pipe IPipeChannel.In
        {
            get { return In; }
        }

        IPipelineFilter IPipeChannel.PipelineFilter
        {
            get { return _pipelineFilter; }
        }

        private IObjectPipe<TPackageInfo> _packagePipe;

        protected ILogger Logger { get; }

        protected ChannelOptions Options { get; }

        private Task _readsTask;

        private Task _sendsTask;

        private bool _isDetaching = false;

        protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
        {
            _pipelineFilter = pipelineFilter;

            if (!options.ReadAsDemand)
                _packagePipe = new DefaultObjectPipe<TPackageInfo>();
            else
                _packagePipe = new DefaultObjectPipeWithSupplyControl<TPackageInfo>();

            Options = options;
            Logger = options.Logger;
            Out = options.Out ?? new Pipe();
            In = options.In ?? new Pipe();
        }

        public override void Start()
        {
            _readsTask = ProcessReads(_cts.Token);
            _sendsTask = ProcessSends();
            WaitHandleClosing();
        }

        private async void WaitHandleClosing()
        {
            await HandleClosing().ConfigureAwait(false);
        }

        public async override IAsyncEnumerable<TPackageInfo> RunAsync()
        { 
            if (_readsTask == null || _sendsTask == null)
                throw new Exception("The channel has not been started yet.");

            while (!_cts.IsCancellationRequested)
            {
                var package = await _packagePipe.ReadAsync().ConfigureAwait(false);

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

        protected virtual async Task FillPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            var options = Options;
            var supplyController = _packagePipe as ISupplyController;

            if (supplyController != null)
            {
                cancellationToken.Register(() =>
                {
                    supplyController.SupplyEnd();
                });
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {                    
                    if (supplyController != null)
                    {
                        await supplyController.SupplyRequired().ConfigureAwait(false);

                        if (cancellationToken.IsCancellationRequested)
                            break;
                    }

                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;

                    if (bufferSize <= 0)
                        bufferSize = 1024 * 4; //4k

                    var memory = writer.GetMemory(bufferSize);

                    var bytesRead = await FillPipeWithDataAsync(memory, cancellationToken).ConfigureAwait(false);       

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
                        if (!(e is OperationCanceledException))
                            OnError("Exception happened in ReceiveAsync", e);

                        if (!CloseReason.HasValue)
                        {
                            CloseReason = cancellationToken.IsCancellationRequested
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
                var result = await writer.FlushAsync().ConfigureAwait(false);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            await writer.CompleteAsync().ConfigureAwait(false);
            // And don't allow writing data to outgoing pipeline
            await Out.Writer.CompleteAsync().ConfigureAwait(false);
        }

        protected virtual bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException || e is NullReferenceException)
                return true;

            if (e.InnerException != null)
                return IsIgnorableException(e.InnerException);

            return false;
        }

        protected abstract ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken);

        protected virtual async Task ProcessReads(CancellationToken cancellationToken)
        {
            var pipe = In;

            Task writing = FillPipeAsync(pipe.Writer, cancellationToken);
            Task reading = ReadPipeAsync(pipe.Reader, cancellationToken);

            await Task.WhenAll(reading, writing).ConfigureAwait(false);
        }

        protected async ValueTask<bool> ProcessOutputRead(PipeReader reader)
        {
            var result = await reader.ReadAsync(CancellationToken.None).ConfigureAwait(false);

            var completed = result.IsCompleted;

            var buffer = result.Buffer;
            var end = buffer.End;
            
            if (!buffer.IsEmpty)
            {
                try
                {
                    await SendOverIOAsync(buffer, CancellationToken.None).ConfigureAwait(false);;
                    LastActiveTime = DateTimeOffset.Now;
                }
                catch (Exception e)
                {
                    // Cancel all the work in the channel if encounter an error during sending
                    Cancel();
                    
                    if (!IsIgnorableException(e))
                        OnError("Exception happened in SendAsync", e);
                    
                    return true;
                }
            }

            reader.AdvanceTo(end);
            return completed;
        }

        protected virtual async Task ProcessSends()
        {
            var output = Out.Reader;

            while (true)
            {
                var completed = await ProcessOutputRead(output).ConfigureAwait(false);

                if (completed)
                {
                    break;
                }
            }

            output.Complete();
        }


        private void CheckChannelOpen()
        {
            if (this.IsClosed)
            {
                throw new Exception("Channel is closed now, send is not allowed.");
            }
        }

        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);


        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            try
            {
                await SendLock.WaitAsync().ConfigureAwait(false);
                var writer = Out.Writer;
                WriteBuffer(writer, buffer);
                await writer.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                SendLock.Release();
            }            
        }

        private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
        {
            CheckChannelOpen();
            writer.Write(buffer.Span);
        }

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            try
            {
                await SendLock.WaitAsync().ConfigureAwait(false);
                var writer = Out.Writer;
                WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);
                await writer.FlushAsync().ConfigureAwait(false);
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
                var writer = Out.Writer;
                write(writer);
                await writer.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                SendLock.Release();
            }
        }

        protected void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            CheckChannelOpen();
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
                    CloseReason = Channel.CloseReason.ProtocolError;
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
            _packagePipe.Write(default);
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
                    CloseReason = Channel.CloseReason.ProtocolError;
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
