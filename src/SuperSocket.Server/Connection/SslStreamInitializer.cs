using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    public class SslStreamInitializer : IConnectionStreamInitializer
    {
        private SslServerAuthenticationOptions _authOptions;

        public void Setup(ListenOptions listenOptions)
        {
            var authOptions = listenOptions.AuthenticationOptions;

            if (authOptions.ServerCertificate == null)
            {
                authOptions.EnsureCertificate();
            }
            
            _authOptions = authOptions;
        }

        public async Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken)
        {
            var sslStream = new SslStream(stream, false);
            await sslStream.AuthenticateAsServerAsync(_authOptions, cancellationToken);
            return sslStream;
        }
    }
}