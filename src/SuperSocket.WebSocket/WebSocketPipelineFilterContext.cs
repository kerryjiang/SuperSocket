using SuperSocket.WebSocket.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace SuperSocket.WebSocket
{
    public class WebSocketPipelineFilterContext
    {
        public IReadOnlyList<IWebSocketExtension> Extensions { get; set; }
    }
}
