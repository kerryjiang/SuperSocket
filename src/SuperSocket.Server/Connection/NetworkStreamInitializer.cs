using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    /// <summary>
    /// Initializes a network stream for socket communication.
    /// </summary>
    public class NetworkStreamInitializer : IConnectionStreamInitializer
    {
        /// <summary>
        /// Configures the network stream initializer with the specified listen options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        public void Setup(ListenOptions listenOptions)
        {
        }

        /// <summary>
        /// Initializes the network stream asynchronously.
        /// </summary>
        /// <param name="socket">The socket associated with the connection.</param>
        /// <param name="stream">The underlying stream (not used in this implementation).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the initialized network stream.</returns>
        public Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(new NetworkStream(socket, true));
        }
    }
}