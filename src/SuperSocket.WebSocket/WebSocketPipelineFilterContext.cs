using System;
using System.Buffers;
using System.Collections.Generic;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket
{
    public class WebSocketPipelineFilterContext
    {
        public IReadOnlyList<IWebSocketExtension> Extensions { get; set; }
    }
}
