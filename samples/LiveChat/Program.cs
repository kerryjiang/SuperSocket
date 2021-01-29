using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.Server;

namespace LiveChat
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
                }).AsWebSocketHostBuilder()
                .UseSession<ChatSession>()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<RoomService>();
                })                
                .UseCommand<StringPackageInfo, StringPackageConverter>(commandOptions =>
                    {
                        commandOptions.AddCommand<CON>();
                        commandOptions.AddCommand<MSG>();
                    });
    }
}
