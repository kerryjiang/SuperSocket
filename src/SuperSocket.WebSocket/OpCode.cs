using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents the operation codes used in WebSocket communication.
    /// </summary>
    public enum OpCode : sbyte
    {
        /// <summary>
        /// Indicates a WebSocket handshake operation.
        /// </summary>
        Handshake = -1,

        /// <summary>
        /// Indicates a continuation frame.
        /// </summary>
        Continuation = 0,

        /// <summary>
        /// Indicates a text frame.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Indicates a binary frame.
        /// </summary>
        Binary = 2,

        /// <summary>
        /// Indicates a connection close frame.
        /// </summary>
        Close = 8,

        /// <summary>
        /// Indicates a ping frame.
        /// </summary>
        Ping = 9,

        /// <summary>
        /// Indicates a pong frame.
        /// </summary>
        Pong = 10
    }
}
