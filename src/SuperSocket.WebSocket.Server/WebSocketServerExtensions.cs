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
            return builder.UseSuperSocketWebSocket<WebSocketService>();
        }

        public static IHostBuilder UseSuperSocketWebSocket<TWebSocketService>(this IHostBuilder builder)
            where TWebSocketService : WebSocketService
        {
            return builder
                .UseSuperSocket<WebSocketPackage, WebSocketPipelineFilter, TWebSocketService>();
        }
    }
}
