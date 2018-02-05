using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Buffers;

namespace SuperSocket.Server
{
    public interface IAppSession
    {
        void Initialize(IDuplexPipe pipe);

        Task ProcessRequest();

        Task SendAsync(ReadOnlyBuffer<byte> buffer);

        event EventHandler Closed;
    }
}