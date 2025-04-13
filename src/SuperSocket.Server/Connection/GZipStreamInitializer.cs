using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    /// <summary>
    /// Initializes a GZip stream for compressing and decompressing data.
    /// </summary>
    public class GZipStreamInitializer : IConnectionStreamInitializer
    {
        /// <summary>
        /// Gets the compression level used for the GZip stream.
        /// </summary>
        public CompressionLevel CompressionLevel { get; private set; }

        /// <summary>
        /// Initializes the GZip stream asynchronously.
        /// </summary>
        /// <param name="socket">The socket associated with the connection.</param>
        /// <param name="stream">The underlying stream to wrap with GZip compression.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the initialized GZip stream.</returns>
        public Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken)
        {
            var connectionStream = new ReadWriteDelegateStream(
                stream,
                new GZipStream(stream, CompressionMode.Decompress),
                new GZipStream(stream, CompressionLevel));
                
            return Task.FromResult<Stream>(connectionStream);
        }

        /// <summary>
        /// Configures the GZip stream initializer with the specified listen options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        public void Setup(ListenOptions listenOptions)
        {
            CompressionLevel = CompressionLevel.Optimal;
        }
    }
}