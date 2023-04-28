using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Server;

var host = WebSocketHostBuilder.Create()
    .UseWebSocketMessageHandler
    (
        async (session, message) =>
        {
            var result = message.OpCode switch
            {
                OpCode.Text => message.Message.Length,
                OpCode.Binary => message.Data.Length,
                _ => 0,
            };

            Console.WriteLine($"消息类型：{message.OpCode},消息长度：{result}");

            await session.SendAsync(message);
        }
    )
    .ConfigureAppConfiguration((hostCtx, configApp) =>
    {
        configApp.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "serverOptions:name", "TestServer" },
            { "serverOptions:listeners:0:ip", "Any" },
            { "serverOptions:listeners:0:port", "4040" }
        });
    })
    .ConfigureLogging((hostCtx, loggingBuilder) =>
    {
        loggingBuilder.AddConsole();
    })
    .Build();

await host.RunAsync();