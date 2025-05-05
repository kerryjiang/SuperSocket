using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents configuration options for loading an X.509 certificate.
    /// </summary>
    public class CertificateOptions
    {
        /// <summary>
        /// Gets or sets the certificate file path (pfx).
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the password for the certificate file.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the name of the store where the certificate is located.
        /// </summary>
        public string StoreName { get; set; } = "My"; // The X.509 certificate store for personal certificates.

        /// <summary>
        /// Gets or sets the thumbprint of the certificate.
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// Gets or sets the store location of the certificate.
        /// </summary>
        public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser; // The X.509 certificate store used by the current user.

        /// <summary>
        /// Gets or sets the key storage flags used to instantiate the X509Certificate2 object.
        /// </summary>
        public X509KeyStorageFlags KeyStorageFlags { get; set; }

        /// <summary>
        /// Retrieves the X.509 certificate based on the specified options.
        /// </summary>
        /// <returns>The loaded <see cref="X509Certificate"/>.</returns>
        /// <exception cref="Exception">Thrown if neither <see cref="FilePath"/> nor <see cref="Thumbprint"/> is provided.</exception>
        public X509Certificate GetCertificate()
        {
            // Load certificate from pfx file
            if (!string.IsNullOrEmpty(FilePath))
            {
                string filePath = FilePath;

                if (!Path.IsPathRooted(filePath))
                {
                    filePath = Path.Combine(AppContext.BaseDirectory, filePath);
                }

                return new X509Certificate2(filePath, Password, KeyStorageFlags);
            }
            else if (!string.IsNullOrEmpty(Thumbprint)) // Load certificate from certificate store
            {
                using var store = new X509Store(Enum.Parse<StoreName>(StoreName), StoreLocation);

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