using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    public interface IConnectionStreamInitializer
    {
        void Setup(ListenOptions listenOptions);

        Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken);
    }
}