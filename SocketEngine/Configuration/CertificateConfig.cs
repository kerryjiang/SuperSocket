using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Certificate configuration
    /// </summary>
    public class CertificateConfig : ConfigurationElement, ICertificateConfig
    {
        #region ICertificateConfig Members

        /// <summary>
        /// Gets the certificate file path.
        /// </summary>
        [ConfigurationProperty("filePath", IsRequired = false)]
        public string FilePath
        {
            get
            {
                return this["filePath"] as string;
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return this["password"] as string;
            }
        }

        /// <summary>
        /// Gets the the store where certificate locates.
        /// </summary>
        /// <value>
        /// The name of the store.
        /// </value>
        [ConfigurationProperty("storeName", IsRequired = false)]
        public string StoreName
        {
            get
            {
                return this["storeName"] as string;
            }
        }

        /// <summary>
        /// Gets the thumbprint.
        /// </summary>
        [ConfigurationProperty("thumbprint", IsRequired = false)]
        public string Thumbprint
        {
            get
            {
                return this["thumbprint"] as string;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [client certificate required].
        /// </summary>
        /// <value>
        /// <c>true</c> if [client certificate required]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("clientCertificateRequired", IsRequired = false, DefaultValue = false)]
        public bool ClientCertificateRequired
        {
            get
            {
                return (bool)this["clientCertificateRequired"];
            }
        }

        #endregion ICertificateConfig Members
    }
}
