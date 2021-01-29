using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace AspNetSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).AsSuperSocketHostBuilder<TextPackageInfo, LinePipelineFilter>()
                .UsePackageHandler(async (s, p) =>
                {
                    // echo message back to client
                    await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
                });
    }
}
