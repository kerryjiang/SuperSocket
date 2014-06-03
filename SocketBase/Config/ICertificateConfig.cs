using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Certificate configuration interface
    /// </summary>
    public interface ICertificateConfig
    {
        /// <summary>
        /// Gets the file path.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the the store where certificate locates.
        /// </summary>
        /// <value>
        /// The name of the store.
        /// </value>
        string StoreName { get; }

        /// <summary>
        /// Gets the thumbprint.
        /// </summary>
        string Thumbprint { get; }


        /// <summary>
        /// Gets the store location of the certificate.
        /// </summary>
        /// <value>
        /// The store location.
        /// </value>
        StoreLocation StoreLocation { get; }


        /// <summary>
        /// Gets a value indicating whether [client certificate required].
        /// </summary>
        /// <value>
        /// <c>true</c> if [client certificate required]; otherwise, <c>false</c>.
        /// </value>
        bool ClientCertificateRequired { get; }

        /// <summary>
        /// Gets a value that will be used to instantiate the X509Certificate2 object in the CertificateManager
        /// </summary>
        X509KeyStorageFlags KeyStorageFlags { get; }
    }
}
