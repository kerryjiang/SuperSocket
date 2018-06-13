using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IPipeChannel<TPackageInfo>, IChannel<TPackageInfo>, IPipeChannel, IChannel
        where TPackageInfo : class
    {
        protected IDuplexPipe Pipe { get; private set; }

        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        private TransportConnection _transportConnection;
        
        public PipeChannel(TransportConnection transportConnection, IPipelineFilter<TPackageInfo> pipelineFilter)
        {
            _transportConnection = transportConnection;
            _pipelineFilter = pipelineFilter;
        }

        public void Initialize(IDuplexPipe pipe)
        {
            Pipe = pipe;
        }

        public override async Task ProcessRequest()
        {
            var input = _transportConnection.Application.Input;

            var currentPipelineFilter = _pipelineFilter;

            while (true)
            {
                var result = await input.ReadAsync();
                var buffer = result.Buffer;

                try
                {
                    if (result.IsCompleted)
                    {
                        OnClosed();
                        break;
                    }

                    while (true)
                    {
                        var packageInfo = currentPipelineFilter.Filter(ref buffer);

                        if (currentPipelineFilter.NextFilter != null)
                            _pipelineFilter = currentPipelineFilter = currentPipelineFilter.NextFilter;
                    
                        // continue receive...
                        if (packageInfo == null)
                            break;

                        // already get a package
                        OnPackageReceived(packageInfo);

                        if (buffer.Length == 0) // no more data
                            break;
                    }
                }
                finally
                {
                    input.AdvanceTo(buffer.Start, buffer.End);
                }
            }

            await Task.CompletedTask;
        }

        public override Task SendAsync(ReadOnlySpan<byte> buffer)
        {
            var pipe = Pipe;
            pipe.Output.Write(buffer);
            return FlushAsync(pipe.Output);
        }

        async Task FlushAsync(PipeWriter buffer)
        {
            await buffer.FlushAsync();
        }
    }
}
