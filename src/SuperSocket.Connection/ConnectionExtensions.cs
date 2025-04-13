using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Provides extension methods for working with connections.
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Gets the remote certificate associated with the connection, if available.
        /// </summary>
        /// <param name="connection">The connection to retrieve the remote certificate from.</param>
        /// <returns>The remote certificate, or <c>null</c> if no certificate is available.</returns>
        public static X509Certificate GetRemoteCertificate(this IConnection connection)
        {
            if (connection is IStreamConnection streamConnection
                && streamConnection.Stream is SslStream sslStream
                && sslStream.IsAuthenticated)
            {
                return sslStream.RemoteCertificate;
            }

            return null;
        }
    }
}