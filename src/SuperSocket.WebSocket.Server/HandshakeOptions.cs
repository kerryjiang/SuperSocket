using System;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    /// <summary>
    /// Represents options for WebSocket handshake operations.
    /// </summary>
    public class HandshakeOptions
    {
        /// <summary>
        /// Gets or sets the handshake queue checking interval, in seconds.
        /// </summary>
        /// <value>Default: 60 seconds.</value>
        public int CheckingInterval { get; set; } = 60;

        /// <summary>
        /// Gets or sets the open handshake timeout, in seconds.
        /// </summary>
        /// <value>Default: 120 seconds.</value>
        public int OpenHandshakeTimeOut { get; set; } = 120;

        /// <summary>
        /// Gets or sets the close handshake timeout, in seconds.
        /// </summary>
        /// <value>Default: 120 seconds.</value>
        public int CloseHandshakeTimeOut { get; set; } = 120;

        /// <summary>
        /// Gets or sets the validator function for WebSocket handshakes.
        /// </summary>
        public Func<WebSocketSession, WebSocketPackage, ValueTask<bool>> HandshakeValidator { get; set; }
    }
}