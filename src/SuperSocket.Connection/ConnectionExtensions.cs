using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Connection
{
    public static class ConnectionExtensions
    {
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