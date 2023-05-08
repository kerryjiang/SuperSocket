using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public abstract class VirtualChannel<TPackageInfo> : PipeChannel<TPackageInfo>, IVirtualChannel
    {
        public VirtualChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
 
        }

        protected override Task FillPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async ValueTask<FlushResult> WritePipeDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await In.Writer.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
        }
    }
}