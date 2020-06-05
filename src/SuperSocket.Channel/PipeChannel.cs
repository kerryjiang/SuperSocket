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

        private SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

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

        protected void StartTasks()
        {
            _readsTask = ProcessReads();
            _sendsTask = ProcessSends();
            WaitHandleClosing();
        }

        private async void WaitHandleClosing()
        {
            await HandleClosing();
        }

        public async override IAsyncEnumerable<TPackageInfo> RunAsync()
        { 
            while (true)
            {
                var package = await _packagePipe.ReadAsync();

                if (package == null)
                {
                    await HandleClosing();
                    yield break;
                }

                yield return package;
            }
        }

        private async ValueTask HandleClosing()
        {
            try
            {
                await Task.WhenAll(_readsTask, _sendsTask);
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
                if (!_isDetaching)
                    OnClosed();
            }
        }

        protected abstract void Close();

        public override async ValueTask CloseAsync()
        {
            Close();
            await HandleClosing();
        }

        protected virtual async Task FillPipeAsync(PipeWriter writer)
        {
            var options = Options;
            var cts = _cts;

            var supplyController = _packagePipe as ISupplyController;

            if (supplyController != null)
            {
                cts.Token.Register(() =>
                {
                    supplyController.SupplyEnd();
                });
            }

            while (!cts.IsCancellationRequested)
            {
                try
                {                    
                    if (supplyController != null)
                    {
                        await supplyController.SupplyRequired();

                        if (cts.IsCancellationRequested)
                            break;
                    }                        

                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;

                    if (bufferSize <= 0)
                        bufferSize = 1024 * 4; //4k

                    var memory = writer.GetMemory(bufferSize);

                    var bytesRead = await FillPipeWithDataAsync(memory, cts.Token);         

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    LastActiveTime = DateTimeOffset.Now;
                    
                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    if (!IsIgnorableException(e))
                        OnError("Exception happened in ReceiveAsync", e);
                    
                    break;
                }

                // Make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            writer.Complete();
            Out.Writer.Complete();// TODO: should complete the output right now?
        }

        protected virtual async Task FillAndReadPipeAsync(PipeWriter writer, PipeReader reader)
        {
            var options = Options;
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

                    var bytesRead = await FillPipeWithDataAsync(memory, cts.Token);         

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    LastActiveTime = DateTimeOffset.Now;
                    
                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    if (!IsIgnorableException(e))
                        OnError("Exception happened in ReceiveAsync", e);
                    
                    break;
                }

                // Make the data available to the PipeReader
                var fr = await writer.FlushAsync();

                if (fr.IsCompleted)
                {
                    break;
                }

                var result = await reader.ReadAsync(cts.Token);

                var buffer = result.Buffer;

                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }

                    var completed = result.IsCompleted;

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
                    Close();
                    break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }

            }

            // Signal to the reader that we're done writing
            writer.Complete();
            Out.Writer.Complete();// TODO: should complete the output right now?
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

        protected virtual async Task ProcessReads()
        {
            var pipe = In;

            Task writing = FillPipeAsync(pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);
        }

        protected async Task ProcessSends()
        {
            var output = Out.Reader;
            var cts = _cts;

            while (!cts.IsCancellationRequested)
            {
                var result = await output.ReadAsync(cts.Token);

                if (result.IsCanceled)
                    break;

                var completed = result.IsCompleted;

                var buffer = result.Buffer;
                var end = buffer.End;
                
                if (!buffer.IsEmpty)
                {
                    try
                    {
                        await SendOverIOAsync(buffer, cts.Token);
                        LastActiveTime = DateTimeOffset.Now;
                    }
                    catch (Exception e)
                    {
                        output.Complete(e);
                        cts.Cancel(false);
                        
                        if (!IsIgnorableException(e))
                            OnError("Exception happened in SendAsync", e);
                        
                        return;
                    }
                }

                output.AdvanceTo(end);

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
                await _sendLock.WaitAsync();
                var writer = Out.Writer;
                WriteBuffer(writer, buffer);
                await writer.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
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
                await _sendLock.WaitAsync();
                var writer = Out.Writer;
                WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);
                await writer.FlushAsync();
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
                var writer = Out.Writer;
                write(writer);
                await writer.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private void WritePackageWithEncoder<TPackage>(PipeWriter writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
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

        protected async Task ReadPipeAsync(PipeReader reader)
        {
            var cts = _cts;

            while (!cts.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cts.Token);

                var buffer = result.Buffer;

                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }

                    var completed = result.IsCompleted;

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

        private void WriteEOFPackage()
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

                var packageInfo = currentPipelineFilter.Filter(ref seqReader);

                var nextFilter = currentPipelineFilter.NextFilter;

                if (nextFilter != null)
                {
                    nextFilter.Context = currentPipelineFilter.Context; // pass through the context
                    _pipelineFilter = nextFilter;
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
                    // close the the connection directly
                    Close();
                    return false;
                }
            
                // continue receive...
                if (packageInfo == null)
                {
                    consumed = buffer.GetPosition(bytesConsumedTotal);
                    return true;
                }

                currentPipelineFilter.Reset();

                _packagePipe.Write(packageInfo);

                if (seqReader.End) // no more data
                {
                    examined = consumed = buffer.End;
                    return true;
                }
                
                seqReader = new SequenceReader<byte>(seqReader.Sequence.Slice(bytesConsumed));
            }
        }
    
        public override async ValueTask DetachAsync()
        {
            _isDetaching = true;
            _cts.Cancel();
            await HandleClosing();
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
