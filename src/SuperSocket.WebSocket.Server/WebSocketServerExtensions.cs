using System;
using Microsoft.Extensions.Hosting;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Server;

namespace SuperSocket
{
    public static class WebSocketServerExtensions
    {
        public static IHostBuilder UseSuperSocketWebSocket(this IHostBuilder builder)
        {
            return builder
                .UseSuperSocket<WebSocketPackage, WebSocketPipelineFilter, WebSocketService>()
                .UseSession<WebSocketSession>();
        }
    }
}
