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
    /// <summary>
    /// Represents a factory for creating WebSocket Per-Message Compression extensions.
    /// </summary>
    /// <remarks>
    /// Implements the WebSocket Per-Message Compression Extension as defined in RFC 7692.
    /// </remarks>
    public class WebSocketPerMessageCompressionExtensionFactory : IWebSocketExtensionFactory
    {
        /// <summary>
        /// Gets the name of the WebSocket extension.
        /// </summary>
        public string Name => WebSocketPerMessageCompressionExtension.PMCE;

        private static readonly NameValueCollection _supportedOptions;

        static WebSocketPerMessageCompressionExtensionFactory()
        {
            _supportedOptions = new NameValueCollection();
            _supportedOptions.Add("client_no_context_takeover", string.Empty);          
        }

        /// <summary>
        /// Creates a WebSocket Per-Message Compression extension based on the specified options.
        /// </summary>
        /// <param name="options">The options for the extension.</param>
        /// <param name="supportedOptions">The supported options for the extension.</param>
        /// <returns>The created WebSocket extension, or null if the options are invalid.</returns>
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
