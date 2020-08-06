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
        string Name { get; }

        void Encode(WebSocketPackage package);

        void Decode(WebSocketPackage package);
    }
}
