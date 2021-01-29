using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace CustomProtocol
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder.Create<MyPackage, MyPackageFilter>(args)
                .UsePackageHandler(async (s, p) =>
                {
                    // handle package
                    await Task.Delay(0);
                })
                .ConfigureSuperSocket(options =>
                {
                    options.Name = "CustomProtocol Server";
                    options.Listeners = new List<ListenOptions>
                    {
                        new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4040
                        }
                    };
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                }).Build();

            await host.RunAsync();
        }
    }
}
