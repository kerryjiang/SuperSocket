using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Certificate configuration interface
    /// </summary>
    public interface ICertificateConfig
    {
        /// <summary>
        /// Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        bool IsEnabled { get; }

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
    }
}
