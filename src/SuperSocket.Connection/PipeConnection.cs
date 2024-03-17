using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Buffers;
using System.Collections.Generic;
using SuperSocket.ProtoBase;
using System.IO.Pipelines;

namespace SuperSocket.Connection
{
    public abstract class PipeConnection<TPackageInfo> : PipeConnectionBase<TPackageInfo>
    {
        public PipeConnection(IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
            : base(pipelineFilter, options)
        {
        }

        protected override Task StartTask()
        {
            var pipeTask = base.StartTask();
            return Task.WhenAll(pipeTask, ProcessSends());
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
                    await SendOverIOAsync(buffer, CancellationToken.None).ConfigureAwait(false); ;
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

        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        protected override bool IsIgnorableException(Exception e)
        {
            if (base.IsIgnorableException(e))
                return true;

            if (e is SocketException se)
            {
                if (se.IsIgnorableSocketException())
                    return true;
            }

            return false;
        }
    }
}
