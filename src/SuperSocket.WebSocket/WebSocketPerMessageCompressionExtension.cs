using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// WebSocket Per-Message Compression Extension
    /// https://tools.ietf.org/html/rfc7692
    /// </summary>
    public abstract class WebSocketPerMessageCompressionExtension : IWebSocketExtension
    {
        public void Decode(ref ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }

        public void Encode(ref ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
