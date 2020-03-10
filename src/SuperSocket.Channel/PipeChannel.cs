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
        where TPackageInfo : class
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

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

        protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
        {
            _pipelineFilter = pipelineFilter;
            _packagePipe = new DefaultObjectPipe<TPackageInfo>();
            Options = options;
            Logger = options.Logger;
            Out = options.Out ?? new Pipe();
            In = options.In ?? new Pipe();
        }

        public async override IAsyncEnumerable<TPackageInfo> RunAsync()
        {
            var readsTask = ProcessReads();
            var sendsTask = ProcessSends();
            
            while (true)
            {
                var package = await _packagePipe.ReadAsync();

                if (package == null)
                {
                    await HandleClosing(readsTask, sendsTask);
                    yield break;
                }

                yield return package;
            }
        }

        private async Task HandleClosing(Task readsTask, Task sendsTask)
        {
            try
            {
                await Task.WhenAll(readsTask, sendsTask);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unhandled exception in the method PipeChannel.Run.");
            }
            finally
            {
                OnClosed();
            }
        }
        protected virtual async Task FillPipeAsync(PipeWriter writer)
        {
            var options = Options;

            while (true)
            {
                try
                {
                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;

                    if (maxPackageLength > 0)
                        bufferSize = Math.Min(bufferSize, maxPackageLength);

                    var memory = writer.GetMemory(bufferSize);

                    var bytesRead = await FillPipeWithDataAsync(memory);         

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
                        Logger.LogError(e, "Exception happened in ReceiveAsync");
                    
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

        protected virtual bool IsIgnorableException(Exception e)
        {
            return false;
        }

        protected abstract ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory);

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

            while (true)
            {
                var result = await output.ReadAsync();

                if (result.IsCanceled)
                    break;

                var completed = result.IsCompleted;

                var buffer = result.Buffer;
                var end = buffer.End;
                
                if (!buffer.IsEmpty)
                {
                    try
                    {
                        await SendOverIOAsync(buffer);
                        LastActiveTime = DateTimeOffset.Now;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Exception happened in SendAsync");
                        output.Complete(e);
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

        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer);


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
            while (true)
            {
                var result = await reader.ReadAsync();

                var buffer = result.Buffer;

                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                try
                {
                    if (result.IsCanceled)
                    {
                        WriteEOFPackage();
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
                        WriteEOFPackage();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogCritical(e, "Protocol error");
                    // close the connection if get a protocol error
                    Close();
                    WriteEOFPackage();
                    break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            reader.Complete();
        }

        private void WriteEOFPackage()
        {
            _packagePipe.Write(null);
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
                    Logger.LogError($"Package cannot be larger than {maxPackageLength}.");
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
    }
}
