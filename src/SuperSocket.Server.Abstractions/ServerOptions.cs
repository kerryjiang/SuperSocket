using System.Collections.Generic;
using System.Text;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents the configuration options for a server.
    /// </summary>
    public class ServerOptions : ConnectionOptions
    {
        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of listener options for the server.
        /// </summary>
        public List<ListenOptions> Listeners { get; set; }

        /// <summary>
        /// Gets or sets the default text encoding for the server.
        /// </summary>
        public Encoding DefaultTextEncoding { get; set; }

        /// <summary>
        /// Gets or sets the interval for clearing idle sessions, in seconds.
        /// </summary>
        public int ClearIdleSessionInterval { get; set; } = 120;

        /// <summary>
        /// Gets or sets the timeout for idle sessions, in seconds.
        /// </summary>
        public int IdleSessionTimeOut { get; set; } = 300;

        /// <summary>
        /// Gets or sets the timeout for package handling, in seconds.
        /// </summary>
        public int PackageHandlingTimeOut { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating whether the proxy protocol is enabled.
        /// </summary>
        public bool EnableProxyProtocol { get; set; }
    }
}