using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Server.Abstractions
{
    public class ServerAuthenticationOptions : SslServerAuthenticationOptions
    {
        /// <summary>
        /// The certificate options.
        /// </summary>
        public CertificateOptions CertificateOptions { get; set; }

        public void EnsureCertificate()
        {
            var certificateOptions = CertificateOptions;

            if (certificateOptions != null)
            {
                ServerCertificate = certificateOptions.GetCertificate();
            }
        }

        public override string ToString()
        {
            return EnabledSslProtocols.ToString();
        }
    }
}