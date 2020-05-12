using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
                .UseWebSocketMessageHandler(async (session, message) =>
                {
                    if (message.Message.Equals("StartPush", StringComparison.OrdinalIgnoreCase))
                    {
                        var server = session.Server as IServer;
                        var sessionCount = server.SessionCount;
                        var serverPush = server.ServiceProvider.GetServices<IMiddleware>().OfType<ServerPushMiddleware>()
                            .FirstOrDefault();
                        serverPush.StartPush(sessionCount);
                        session.GetDefaultLogger().LogInformation($"Start pushing to {sessionCount} clients...");
                        return;
                    }

                    // echo message back to the client
                    await session.SendAsync(message.Message);
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
