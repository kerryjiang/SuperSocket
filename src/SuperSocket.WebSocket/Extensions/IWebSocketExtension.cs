using System;
using System.Buffers;

namespace SuperSocket.WebSocket.Extensions
{
    /// <summary>
    /// WebSocket Extensions
    /// https://tools.ietf.org/html/rfc6455#section-9
    /// </summary>
    public interface IWebSocketExtension
    {
        /// <summary>
        /// Gets the name of the WebSocket extension.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Encodes a WebSocket package using the extension.
        /// </summary>
        /// <param name="package">The WebSocket package to encode.</param>
        void Encode(WebSocketPackage package);

        /// <summary>
        /// Decodes a WebSocket package using the extension.
        /// </summary>
        /// <param name="package">The WebSocket package to decode.</param>
        void Decode(WebSocketPackage package);
    }
}
