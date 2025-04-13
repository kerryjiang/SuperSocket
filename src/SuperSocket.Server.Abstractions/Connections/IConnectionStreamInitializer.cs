using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Defines an initializer for connection streams.
    /// </summary>
    public interface IConnectionStreamInitializer
    {
        /// <summary>
        /// Sets up the initializer with the specified listen options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        void Setup(ListenOptions listenOptions);

        /// <summary>
        /// Initializes a connection stream asynchronously.
        /// </summary>
        /// <param name="socket">The socket associated with the connection.</param>
        /// <param name="stream">The existing stream to initialize.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken);
    }
}