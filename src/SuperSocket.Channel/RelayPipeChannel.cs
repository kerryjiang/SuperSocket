using System;
using System.Buffers;
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

        public override void Close()
        {
            In.Writer.Complete();
            Out.Writer.Complete();
        }

        protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory)
        {
            throw new NotSupportedException();
        }
    }
}