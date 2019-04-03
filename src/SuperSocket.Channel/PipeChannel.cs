using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;


namespace SuperSocket.Channel
{
    public abstract class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IChannel<TPackageInfo>, IChannel
        where TPackageInfo : class
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        protected Pipe Output { get; }

        protected ILogger Logger { get; }

        protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ILogger logger)
        {
            _pipelineFilter = pipelineFilter;
            Logger = logger;
            Output = new Pipe();
        }

        public override async Task StartAsync()
        {
            try
            {
                var readsTask = ProcessReads();
                var sendsTask = ProcessSends();

                await Task.WhenAll(readsTask, sendsTask);
                OnClosed();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unhandled exception in the method PipeChannel.StartAsync.");
            }
        }

        protected abstract Task ProcessReads();

        protected async Task ProcessSends()
        {
            var output = Output.Reader;

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
                        await SendAsync(buffer);
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

        protected abstract ValueTask<int> SendAsync(ReadOnlySequence<byte> buffer);


        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            var writer = Output.Writer;
            await writer.WriteAsync(buffer);
            await writer.FlushAsync();
        }

        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            var writer = Output.Writer;
            packageEncoder.Encode(writer, package);
            await writer.FlushAsync();
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
                        break;

                    var completed = result.IsCompleted;

                    while (true)
                    {
                        var package = ReaderBuffer(buffer, out consumed, out examined);

                        if (package != null)
                        {
                            await OnPackageReceived(package);
                        }

                        if (examined.Equals(buffer.End))
                            break;

                        buffer = buffer.Slice(examined);
                    }

                    if (completed)
                        break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            reader.Complete();
        }

        private TPackageInfo ReaderBuffer(ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var seqReader = new SequenceReader<byte>(buffer);
            var currentPipelineFilter = _pipelineFilter;

            var packageInfo = currentPipelineFilter.Filter(ref seqReader);

            if (currentPipelineFilter.NextFilter != null)
                _pipelineFilter = currentPipelineFilter.NextFilter;
        
            // continue receive...
            if (packageInfo == null)
                return null;

            currentPipelineFilter.Reset();

            if (seqReader.End) // no more data
            {
                consumed = buffer.End;
            }
            else
            {
                examined = consumed = seqReader.Position;
            }

            return packageInfo;
        }
    }
}
