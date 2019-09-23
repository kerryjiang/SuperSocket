using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.ProtoBase;


namespace SuperSocket.Channel
{
    public abstract class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IChannel<TPackageInfo>, IChannel
        where TPackageInfo : class
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        private LinkedList<TPackageInfo> _receivedPackages = new LinkedList<TPackageInfo>();

        private TaskCompletionSource<bool> _waitForNewPackageTaskSource = new TaskCompletionSource<bool>();

        protected Pipe Output { get; }

        protected ILogger Logger { get; }

        protected ChannelOptions Options { get; }

        protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, ILogger logger)
        {
            _pipelineFilter = pipelineFilter;
            Options = options;
            Logger = logger;
            Output = new Pipe();
        }

        public override async Task StartAsync()
        {
            try
            {
                var readsTask = ProcessReads();
                var sendsTask = ProcessSends();
                var processTask = ProcessPackages();

                await Task.WhenAll(readsTask, sendsTask);

                OnIOClosed();
                
                await processTask;                
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unhandled exception in the method PipeChannel.StartAsync.");
            }
            finally
            {
                OnClosed();
            }
        }

        private void OnIOClosed()
        {
            _waitForNewPackageTaskSource?.SetResult(false);
        }

        async Task ProcessPackages()
        {
            var receivedPackages = _receivedPackages;

            var result = await _waitForNewPackageTaskSource.Task;

            if (!result)
                return;

            while (true)
            {
                var package = default(TPackageInfo);

                lock (receivedPackages)
                {
                    if (receivedPackages.Count > 0)
                    {
                        package = receivedPackages.First.Value;
                        receivedPackages.RemoveFirst();

                        if (receivedPackages.Count == 0)
                        {
                            _waitForNewPackageTaskSource = new TaskCompletionSource<bool>();
                        }
                    }
                }

                if (package != null)
                {
                    await OnPackageReceived(package);
                }
                else
                {
                    result = await _waitForNewPackageTaskSource.Task;

                    if (!result)
                    {
                        break;
                    }
                }
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

                    if (buffer.Length > 0)
                    {
                        if (!ReaderBuffer(buffer, out consumed, out examined))
                            completed = true;
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

        private bool ReaderBuffer(ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            var maxPackageLength = Options.MaxPackageLength;

            var seqReader = new SequenceReader<byte>(buffer);

            while (true)
            {
                var currentPipelineFilter = _pipelineFilter;

                var packageInfo = currentPipelineFilter.Filter(ref seqReader);

                if (currentPipelineFilter.NextFilter != null)
                    _pipelineFilter = currentPipelineFilter.NextFilter;

                var pos = seqReader.Position.GetInteger();

                if (maxPackageLength > 0 && pos > maxPackageLength)
                {
                    Logger.LogError($"Package cannot be larger than {maxPackageLength}.");
                    // close the the connection directly
                    Close();
                    return false;
                }
            
                // continue receive...
                if (packageInfo == null)
                {
                    continue;
                }

                currentPipelineFilter.Reset();

                lock (_receivedPackages)
                {
                    _receivedPackages.AddLast(packageInfo);

                    if (_receivedPackages.Count == 1)
                    {
                        _waitForNewPackageTaskSource.SetResult(true);
                    }
                }

                if (seqReader.End) // no more data
                {
                    consumed = buffer.End;
                    break;
                }
                else
                {
                    examined = consumed = seqReader.Position;
                }
            }

            return true;        
        }
    }
}
