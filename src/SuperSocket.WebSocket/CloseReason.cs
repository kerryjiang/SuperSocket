using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents the reasons for closing a WebSocket connection.
    /// </summary>
    public enum CloseReason : short
    {
        /// <summary>
        /// Indicates a normal closure of the WebSocket connection.
        /// </summary>
        NormalClosure = 1000,

        /// <summary>
        /// Indicates that the WebSocket connection is going away.
        /// </summary>
        GoingAway = 1001,

        /// <summary>
        /// Indicates a protocol error occurred.
        /// </summary>
        ProtocolError = 1002,

        /// <summary>
        /// Indicates that the data received is not acceptable.
        /// </summary>
        NotAcceptableData = 1003,

        /// <summary>
        /// Indicates that the frame received is too large.
        /// </summary>
        TooLargeFrame = 1009,

        /// <summary>
        /// Indicates that the data received contains invalid UTF-8.
        /// </summary>
        InvalidUTF8 = 1007,

        /// <summary>
        /// Indicates a policy violation occurred.
        /// </summary>
        ViolatePolicy = 1008,

        /// <summary>
        /// Indicates that the WebSocket extension does not match.
        /// </summary>
        ExtensionNotMatch = 1010,

        /// <summary>
        /// Indicates an unexpected condition caused the closure.
        /// </summary>
        UnexpectedCondition = 1011,

        /// <summary>
        /// Indicates that no status code was provided.
        /// </summary>
        NoStatusCode = 1005
    }
}
