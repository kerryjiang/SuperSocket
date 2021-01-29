using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
            })
            .AsSuperSocketHostBuilder<TextPackageInfo, LinePipelineFilter>()
            .UsePackageHandler(async (s, p) =>
            {
                // echo message back to client
                await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
            })
            .UseInProcSessionContainer();
    }
}
