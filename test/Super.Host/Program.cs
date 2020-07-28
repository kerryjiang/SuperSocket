using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Super.Engine;
using SuperSocket;
using SuperSocket.Server;
using System.Threading.Tasks;

namespace Super.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole()
                .WriteTo.File("Logs/logs.txt")
                .CreateLogger();

            var host = CreateSocketServerBuilder(args).Build();
            await host.RunAsync();
        }

        static IHostBuilder CreateSocketServerBuilder(string[] args) =>
           MultipleServerHostBuilder.Create()
               .AddServer<OnlinePackageInfo, OnlinePackagePipelineFilter>(onlineServerBuilder =>
               {
                   onlineServerBuilder
                      .ConfigureServerOptions((ctx, config) => config.GetSection("onlineServer"))
                      //.UseHostedService<OnlineService>()
                      .UseCommand<short, OnlinePackageInfo>()
                      //.UseSession<OnlineSession>()
                      .UseSessionFactory<OnlineSessionFactory>()
                      .UseInProcSessionContainer();
               })
               //.AddServer<RemotePackageInfo, RemotePackagePipelineFilter>(onlineServerBuilder =>
               // {
               //     onlineServerBuilder
               //        .ConfigureServerOptions((ctx, config) => config.GetSection("remoteServer"))
               //        .UseCommand<string, RemotePackageInfo>()                      
               //        .UseInProcSessionContainer();
               // })
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>()
                             .UseUrls("http://*:4050")
                             .UseStaticWebAssets();
               })
               //.UseWindowsService()
               .UseAutofac()
               .UseSerilog();
    }
}
