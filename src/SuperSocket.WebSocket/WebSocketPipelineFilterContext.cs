using System;
using System.Buffers;
using System.Collections.Generic;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents the context for a WebSocket pipeline filter, including extensions.
    /// </summary>
    public class WebSocketPipelineFilterContext
    {
        /// <summary>
        /// Gets or sets the list of WebSocket extensions associated with the pipeline filter.
        /// </summary>
        public IReadOnlyList<IWebSocketExtension> Extensions { get; set; }
    }
}
