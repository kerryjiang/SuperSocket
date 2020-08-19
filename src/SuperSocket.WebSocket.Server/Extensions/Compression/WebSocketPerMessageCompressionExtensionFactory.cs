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

        private static readonly NameValueCollection _supportedOptions;

        static WebSocketPerMessageCompressionExtensionFactory()
        {
            _supportedOptions = new NameValueCollection();
            _supportedOptions.Add("client_no_context_takeover", string.Empty);          
        }

        public IWebSocketExtension Create(NameValueCollection options, out NameValueCollection supportedOptions)
        {
            supportedOptions = _supportedOptions;
            
            if (options != null && options.Count > 0)
            {
                foreach (var key in options.AllKeys)
                {
                    if (key.StartsWith("server_", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(options.Get(key)))
                            return null;
                    }
                }
            }

            return new WebSocketPerMessageCompressionExtension();
        }
    }
}
