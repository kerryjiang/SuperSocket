using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Net.Sockets;
using SuperSocket.ProtoBase;
using System.Runtime.InteropServices;

namespace SuperSocket.Channel
{
    public abstract class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IChannel<TPackageInfo>, IChannel
        where TPackageInfo : class
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;
        
        public PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter)
        {
            _pipelineFilter = pipelineFilter;
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
            var currentPipelineFilter = _pipelineFilter;

            while (true)
            {
                var result = await reader.ReadAsync();
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
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }

            reader.Complete();
        }
    }
}
