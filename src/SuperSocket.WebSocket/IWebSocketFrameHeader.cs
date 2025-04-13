using System;
using System.Buffers;
using System.Collections.Specialized;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents the header of a WebSocket frame.
    /// </summary>
    public interface IWebSocketFrameHeader
    {
        /// <summary>
        /// Gets a value indicating whether the FIN (final fragment) bit is set.
        /// </summary>
        bool FIN { get; }

        /// <summary>
        /// Gets a value indicating whether the RSV1 (reserved 1) bit is set.
        /// </summary>
        bool RSV1 { get; }

        /// <summary>
        /// Gets a value indicating whether the RSV2 (reserved 2) bit is set.
        /// </summary>
        bool RSV2 { get; }

        /// <summary>
        /// Gets a value indicating whether the RSV3 (reserved 3) bit is set.
        /// </summary>
        bool RSV3 { get; }
    }
}
