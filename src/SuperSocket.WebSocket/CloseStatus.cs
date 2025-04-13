using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents the status of a WebSocket close operation.
    /// </summary>
    public class CloseStatus
    {
        /// <summary>
        /// Gets or sets the reason for the WebSocket close operation.
        /// </summary>
        public CloseReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the reason text for the WebSocket close operation.
        /// </summary>
        public string ReasonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the close operation was initiated by the remote endpoint.
        /// </summary>
        public bool RemoteInitiated { get; set; }
    }
}
