using System.IO.Pipelines;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents configuration options for managing connections.
    /// </summary>
    public class ConnectionOptions
    {
        /// <summary>
        /// Gets or sets the maximum package length in bytes. Default is 1 MB.
        /// </summary>
        public int MaxPackageLength { get; set; } = 1024 * 1024;

        /// <summary>
        /// Gets or sets the size of the receive buffer in bytes. Default is 4 KB.
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024 * 4;

        /// <summary>
        /// Gets or sets the size of the send buffer in bytes. Default is 4 KB.
        /// </summary>
        public int SendBufferSize { get; set; } = 1024 * 4;

        /// <summary>
        /// Gets or sets a value indicating whether data should be read only when the stream is being consumed.
        /// </summary>
        public bool ReadAsDemand { get; set; }

        /// <summary>
        /// Gets or sets the receive timeout in milliseconds.
        /// </summary>
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets the send timeout in milliseconds.
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the logger for the connection.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the input pipe for the connection.
        /// </summary>
        public Pipe Input { get; set; }

        /// <summary>
        /// Gets or sets the output pipe for the connection.
        /// </summary>
        public Pipe Output { get; set; }

        /// <summary>
        /// Gets or sets additional key-value pairs for connection configuration.
        /// </summary>
        public Dictionary<string, string> Values { get; set; }
    }
}
