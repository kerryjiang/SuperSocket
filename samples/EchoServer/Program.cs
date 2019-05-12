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

namespace EchoServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>()
                .ConfigurePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                })
                .ConfigureSuperSocket(options =>
                {
                    options.Name = "Echo Server";
                    options.Listeners = new [] {
                        new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4040
                        }
                    };
                }).Build();

            await host.RunAsync();
        }
    }
}
