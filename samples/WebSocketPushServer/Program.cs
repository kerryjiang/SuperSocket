using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Server;
using SuperSocket.WebSocket.Server;

namespace WebSocketPushServer
{
    class Program
    {
        static async Task Main(string[] args)
        {               
            var host = WebSocketHostBuilder.Create()
                .ConfigureWebSocketMessageHandler(async (session, message) =>
                {
                    // echo message back to the client
                    await session.SendAsync(message.Message);
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    // register your logging library here
                    loggingBuilder.AddConsole();
                })
                .UseSession<PushSession>()
                .UseInProcSessionContainer()
                .Build();

            await host.RunAsync();
        }
    }
}
