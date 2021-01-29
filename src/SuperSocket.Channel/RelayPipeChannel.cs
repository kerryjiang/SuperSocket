using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class RelayPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
        where TPackageInfo : class
    {
        static ChannelOptions RebuildOptionsWithPipes(ChannelOptions options, Pipe pipeIn, Pipe pipeOut)
        {
            options.In = pipeIn;
            options.Out = pipeOut;
            return options;
        }

        public RelayPipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, Pipe pipeIn, Pipe pipeOut)
            : base(pipelineFilter, RebuildOptionsWithPipes(options, pipeIn, pipeOut))
        {

        }

        protected override void Close()
        {
            In.Writer.Complete();
            Out.Writer.Complete();
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var writer = Out.Writer;
            var total = 0;

            foreach (var data in buffer)
            {
                var result = await writer.WriteAsync(data, cancellationToken);

                if (result.IsCompleted)
                    total += data.Length;
                else if (result.IsCanceled)
                    break;
            }

            return total;
        }

        protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}