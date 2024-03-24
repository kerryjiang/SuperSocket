using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Benchmarks
{
    [ThreadingDiagnoser]
    public class ConcurrentSend
    {
        [Params(100, 500, 1000)]
        public int Iteration;

        [Params(10, 100, 500, 1000, 5000)]
        public int ConcurrentLevel;


        [Benchmark]
        public async Task SendTest()
        {
            var pool = ArrayPool<byte>.Create();
            var tasks = new Task[ConcurrentLevel];
            
            for (var i = 0; i < ConcurrentLevel; i++)
            {
                tasks[i] = Send(pool, new ConnectionOptions(), Iteration);
            }

            await Task.WhenAll(tasks);
        }

        private async Task Send(ArrayPool<byte> pool, ConnectionOptions options, int iteration)
        {
            var connection = new TransparentPipeConnection(options);
            
            _ = connection.RunAsync(new CommandLinePipelineFilter());

            for (var i = 0; i < iteration; i++)
            {
                var text = Guid.NewGuid().ToString() + "\r\n";
                var buffer = pool.Rent(Encoding.UTF8.GetMaxByteCount(text.Length));
                await connection.SendAsync(new ReadOnlyMemory<byte>(buffer));
                pool.Return(buffer);
            }

            await connection.CloseAsync(CloseReason.ServerShutdown);
        }
    }
}
