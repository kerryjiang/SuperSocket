using System;
using System.Threading.Tasks;
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
                .ConfigurePackageHandler(async (s, p) =>
                {
                    // handle package
                    await Task.Delay(0);
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                })
                .ConfigureSuperSocket(options =>
                {
                    options.Name = "CustomProtocol Server";
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
