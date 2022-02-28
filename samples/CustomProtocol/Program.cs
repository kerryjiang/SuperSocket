using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using System.Text;

namespace CustomProtocol
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var encoder = new MyPackageEncoder();
            var host = SuperSocketHostBuilder.Create<MyPackage, MyPackageFilter>()
                .ConfigurePackageHandler(async (s, p) =>
                {
                    // handle package
                    await s.SendAsync(encoder, p);
                    //await Task.Delay(0);
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
