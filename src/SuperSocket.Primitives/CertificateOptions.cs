using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Server.Abstractions
{
    public class CertificateOptions
    {
        /// <summary>
        /// Gets the certificate file path (pfx).
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets the the store where certificate locates.
        /// </summary>
        /// <value>
        /// The name of the store.
        /// </value>
        public string StoreName { get; set; } = "My";//The X.509 certificate store for personal certificates.

        /// <summary>
        /// Gets the thumbprint.
        /// </summary>
        public string Thumbprint { get; set; }


        /// <summary>
        /// Gets the store location of the certificate.
        /// </summary>
        /// <value>
        /// The store location.
        /// </value>
        public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;//The X.509 certificate store used by the current user.


        /// <summary>
        /// Gets a value that will be used to instantiate the X509Certificate2 object in the CertificateManager
        /// </summary>
        public X509KeyStorageFlags KeyStorageFlags { get; set; }

        public X509Certificate GetCertificate()
        {
            // load certificate from pfx file
            if (!string.IsNullOrEmpty(FilePath))
            {
                string filePath = FilePath;

                if (!Path.IsPathRooted(filePath))
                {
                    filePath = Path.Combine(AppContext.BaseDirectory, filePath);
                }

                return new X509Certificate2(filePath, Password, KeyStorageFlags);
            }
            else if (!string.IsNullOrEmpty(Thumbprint)) // load certificate from certificate store
            {
                using var store = new X509Store((StoreName)Enum.Parse(typeof(StoreName), StoreName), StoreLocation);

                store.Open(OpenFlags.ReadOnly);

                return store.Certificates.OfType<X509Certificate2>()
                    .FirstOrDefault(c => c.Thumbprint.Equals(Thumbprint, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                throw new Exception($"Either {FilePath} or {Thumbprint} is required to load the certificate.");
            }
        }
    }
}