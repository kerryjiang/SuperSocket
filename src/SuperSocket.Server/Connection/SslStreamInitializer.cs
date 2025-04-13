using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    /// <summary>
    /// Initializes an SSL stream for secure communication.
    /// </summary>
    public class SslStreamInitializer : IConnectionStreamInitializer
    {
        private SslServerAuthenticationOptions _authOptions;

        /// <summary>
        /// Configures the SSL stream initializer with the specified listen options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener, including authentication settings.</param>
        public void Setup(ListenOptions listenOptions)
        {
            var authOptions = listenOptions.AuthenticationOptions;

            if (authOptions.ServerCertificate == null)
            {
                authOptions.EnsureCertificate();
            }
            
            _authOptions = authOptions;
        }

        /// <summary>
        /// Initializes the SSL stream asynchronously.
        /// </summary>
        /// <param name="socket">The socket associated with the connection.</param>
        /// <param name="stream">The underlying stream to wrap with SSL.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the initialized SSL stream.</returns>
        public async Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken)
        {
            var sslStream = new SslStream(stream, false);
            await sslStream.AuthenticateAsServerAsync(_authOptions, cancellationToken);
            return sslStream;
        }
    }
}