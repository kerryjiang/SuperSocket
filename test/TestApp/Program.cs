using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
        static IHostBuilder CreateSocketServerBuilder()
        {
            return SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>()
                .ConfigurePackageHandler(async (s, p) =>
                {
                    await s.Channel.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
                })
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
                });
        }

        static async Task Main(string[] args)
        {
            var host = CreateSocketServerBuilder().Build();        
            await host.RunAsync();
        }
    }
}
