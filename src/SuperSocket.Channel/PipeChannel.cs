using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;
using System.Runtime.InteropServices;

namespace SuperSocket.Channel
{
    public abstract class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IChannel<TPackageInfo>, IChannel
        where TPackageInfo : class
    {
        private IPipelineFilter<TPackageInfo> _pipelineFilter;

        protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter)
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
            while (true)
            {
                var result = await reader.ReadAsync();

                var buffer = result.Buffer;

                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                try
                {
                    if (result.IsCompleted)
                        break;

                    while (true)
                    {
                        ReaderBuffer(buffer, out consumed, out examined);

                        if (examined.Equals(buffer.End))
                            break;

                        buffer = buffer.Slice(examined);
                    }
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            reader.Complete();
        }

        private void ReaderBuffer(ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
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
                return;

            // already get a package
            OnPackageReceived(packageInfo);

            if (seqReader.End) // no more data
            {
                consumed = buffer.End;
            }
            else
            {
                examined = consumed = seqReader.Position;
            }
        }
    }
}
