using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Server;
using SuperSocket.Server.Host;
using SuperSocket.ProtoBase;

namespace ConfigSample
{
    class Program
    {
        static async Task Main(string[] args)
        {               
            var host = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>(args)
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
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
