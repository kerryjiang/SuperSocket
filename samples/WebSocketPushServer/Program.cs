using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Server.Host;
using SuperSocket.WebSocket.Server;

namespace WebSocketPushServer
{
    class Program
    {
        static async Task Main(string[] args)
        {               
            var host = WebSocketHostBuilder.Create(args)
                .UseWebSocketMessageHandler(async (session, message) =>
                {
                    var s = session as PushSession;

                    if (message.Message.Equals("ACK", StringComparison.OrdinalIgnoreCase))
                    {
                        s.Ack();
                    }

                    await Task.CompletedTask;
                })
                .UseSession<PushSession>()
                .UseInProcSessionContainer()
                .UseMiddleware<ServerPushMiddleware>()
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    // register your logging library here
                    loggingBuilder.AddConsole();
                })                
                .Build();

            await host.RunAsync();
        }
    }
}
