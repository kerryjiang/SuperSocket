using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    public abstract class VirtualConnection<TPackageInfo> : PipeConnection<TPackageInfo>, IVirtualConnection
    {
        public VirtualConnection(IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
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