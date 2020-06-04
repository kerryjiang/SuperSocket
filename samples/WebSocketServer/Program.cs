﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Server;

namespace WebSocketServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = WebSocketHostBuilder.Create()
                .UseWebSocketMessageHandler(async (session, message) =>
                {
                    // echo message back to the client
                    await session.SendAsync(message.Message);
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    // register your logging library here
                    loggingBuilder.AddConsole();
                }).Build();

            await host.RunAsync();
        }
    }
}
