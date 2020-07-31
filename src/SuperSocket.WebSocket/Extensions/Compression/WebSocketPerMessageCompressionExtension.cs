using System;
using System.Buffers;

namespace SuperSocket.WebSocket.Extensions.Compression
{
    /// <summary>
    /// WebSocket Per-Message Compression Extension
    /// https://tools.ietf.org/html/rfc7692
    /// </summary>
    public class WebSocketPerMessageCompressionExtension : IWebSocketExtension
    {
        public string Name => PMCE;

        public const string PMCE = "permessage-deflat";

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
