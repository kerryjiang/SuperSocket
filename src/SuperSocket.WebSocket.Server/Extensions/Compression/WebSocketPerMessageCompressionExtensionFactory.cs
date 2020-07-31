using System;
using System.Buffers;
using System.Collections.Specialized;
using SuperSocket.WebSocket.Extensions;
using SuperSocket.WebSocket.Extensions.Compression;

namespace SuperSocket.WebSocket.Server.Extensions.Compression
{
    /// <summary>
    /// WebSocket Per-Message Compression Extension
    /// https://tools.ietf.org/html/rfc7692
    /// </summary>
    public class WebSocketPerMessageCompressionExtensionFactory : IWebSocketExtensionFactory
    {
        public string Name => WebSocketPerMessageCompressionExtension.PMCE;

        public IWebSocketExtension Create(NameValueCollection options)
        {
            if (options != null && options.Count > 0)
                return null;

            return new WebSocketPerMessageCompressionExtension();
        }
    }
}
