using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Net.Sockets;
using SuperSocket.ProtoBase;
using System.Runtime.InteropServices;

namespace SuperSocket.Channel
{
    public class TcpPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
        where TPackageInfo : class
    {

        private Socket _socket;
        
        public TcpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter)
            : base(pipelineFilter)
        {
            _socket = socket;
        }

        private async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                try
                {
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);

                    int bytesRead = await ReceiveAsync(socket, memory, SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch
                {
                    break;
                }

                // Make the data available to the PipeReader
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            writer.Complete();
        }

        private async Task<int> ReceiveAsync(Socket socket, Memory<byte> memory, SocketFlags socketFlags)
        {
            return await socket.ReceiveAsync(GetArrayByMemory((ReadOnlyMemory<byte>)memory), socketFlags);
        }

        public override async Task ProcessRequest()
        {
            var pipe = new Pipe();

            Task writing = FillPipeAsync(_socket, pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);

            _socket = null;
            OnClosed();
        }
        public override async Task<int> SendAsync(ReadOnlyMemory<byte> buffer)
        {
            return await _socket.SendAsync(GetArrayByMemory(buffer), SocketFlags.None);
        }
    }
}
