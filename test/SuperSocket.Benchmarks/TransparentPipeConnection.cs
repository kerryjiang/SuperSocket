using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Benchmarks
{
    public class TransparentPipeConnection : PipeConnection
    {
        private TaskCompletionSource<int> _tcs;
        private Task<int> _connectionTask;

        public TransparentPipeConnection(ConnectionOptions options)
            : base(options)
        {
            _tcs = new TaskCompletionSource<int>();
            _connectionTask = _tcs.Task;
        }

        public override ValueTask CloseAsync(CloseReason closeReason)
        {
            _tcs.SetResult(0);
            return base.CloseAsync(closeReason);
        }

        protected override void Close()
        {

        }

        protected override async ValueTask<int> FillInputPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            await _connectionTask;
            return 0;
        }

        protected override ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            return new ValueTask<int>((int)buffer.Length);
        }
    }
}