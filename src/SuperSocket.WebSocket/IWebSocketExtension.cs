using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// WebSocket Extensions
    /// https://tools.ietf.org/html/rfc6455#section-9
    /// </summary>
    public interface IWebSocketExtension
    {
        void Encode(ref ReadOnlySequence<byte> data);

        void Decode(ref ReadOnlySequence<byte> data);
    }
}
