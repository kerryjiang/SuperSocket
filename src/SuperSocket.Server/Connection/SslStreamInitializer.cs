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
            var authOptions = new SslServerAuthenticationOptions();

            authOptions.EnabledSslProtocols = listenOptions.Security;

            if (listenOptions.CertificateOptions.Certificate == null)
            {
                listenOptions.CertificateOptions.EnsureCertificate();
            }
            
            authOptions.ServerCertificate = listenOptions.CertificateOptions.Certificate;
            authOptions.ClientCertificateRequired = listenOptions.CertificateOptions.ClientCertificateRequired;

            if (listenOptions.CertificateOptions.RemoteCertificateValidationCallback != null)
                authOptions.RemoteCertificateValidationCallback = listenOptions.CertificateOptions.RemoteCertificateValidationCallback;

            _authOptions = authOptions;
        }

        public async Task<Stream> InitializeAsync(object connection, CancellationToken cancellationToken)
        {
            var stream = (Stream)connection;
            
            var sslStream = new SslStream(stream, false);
            await sslStream.AuthenticateAsServerAsync(_authOptions, cancellationToken);
            return sslStream;
        }
    }
}