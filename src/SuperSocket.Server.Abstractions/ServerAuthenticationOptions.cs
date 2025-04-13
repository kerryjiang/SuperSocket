using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents the authentication options for a server.
    /// </summary>
    public class ServerAuthenticationOptions : SslServerAuthenticationOptions
    {
        /// <summary>
        /// Gets or sets the certificate options for the server.
        /// </summary>
        public CertificateOptions CertificateOptions { get; set; }

        /// <summary>
        /// Ensures that the server certificate is set based on the certificate options.
        /// </summary>
        public void EnsureCertificate()
        {
            var certificateOptions = CertificateOptions;

            if (certificateOptions != null)
            {
                ServerCertificate = certificateOptions.GetCertificate();
            }
        }

        /// <summary>
        /// Returns a string representation of the authentication options.
        /// </summary>
        /// <returns>A string representation of the authentication options.</returns>
        public override string ToString()
        {
            return EnabledSslProtocols.ToString();
        }
    }
}