using System;
using Microsoft.Extensions.Hosting;
using SuperSocket.WebSocket;

namespace SuperSocket
{
    public static class WebSocketServerExtensions
    {
        public static IHostBuilder UseSuperSocketWebSocket(this IHostBuilder builder)
        {
            return builder.UseSuperSocket<WebSocketPackage, WebSocketPipelineFilter>();
        }
    }
}
