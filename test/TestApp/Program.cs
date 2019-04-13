using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace TestApp
{
    class Program
    {
        static IHostBuilder CreateSocketServerBuilder<TPackageInfo, TPipelineFilter>(Dictionary<string, string> configDict = null, Func<IAppSession, TPackageInfo, Task> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            var hostBuilder = new HostBuilder()
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
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddOptions();
                    services.Configure<ServerOptions>(hostCtx.Configuration.GetSection("serverOptions"));
                })
                .UseSuperSocket<TPackageInfo, TPipelineFilter>();

            return hostBuilder;
        }

        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
                .ConfigurePackageHandler(async (IAppSession s, TextPackageInfo p) =>
                {
                    await s.Channel.SendAsync(Encoding.UTF8.GetBytes(p.Text).AsMemory());
                }).Build() as IServer;
            
            await server.StartAsync();

            Console.WriteLine("The server is started.");

            while (Console.ReadLine().ToLower() != "c")
            {
                continue;
            }
            
            await server.StopAsync();
        }
    }
}
