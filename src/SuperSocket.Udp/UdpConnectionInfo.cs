using System;
using System.Net;
using System.Net.Sockets;
using SuperSocket.Connection;

namespace SuperSocket.Udp
{
    /// <summary>
    /// Represents information about a UDP connection.
    /// </summary>
    internal struct UdpConnectionInfo
    {
        /// <summary>
        /// Gets or sets the socket associated with the connection.
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// Gets or sets the connection options.
        /// </summary>
        public ConnectionOptions ConnectionOptions { get; set; }

        /// <summary>
        /// Gets or sets the session identifier for the connection.
        /// </summary>
        public string SessionIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the remote endpoint of the connection.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; }
    }
}