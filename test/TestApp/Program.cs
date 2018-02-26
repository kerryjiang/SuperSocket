using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace TestApp
{
    class Program
    {
        static AppServer CreateSocketServer<TPackageInfo, TPipelineFilter>(Dictionary<string, string> configDict = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            if (configDict == null)
            {
                configDict = new Dictionary<string, string>
                {
                    { "name", "TestServer" },
                    { "listeners:0:ip", "Any" },
                    { "listeners:0:port", "4040" }
                };
            }

            var server = new AppServer();

            var services = new ServiceCollection();

            var builder = new ConfigurationBuilder().AddInMemoryCollection(configDict);
            var config = builder.Build();
            
            services.AddSingleton(new LoggerFactory().AddConsole());
            services.AddLogging();

            RegisterServices(services);

            server.Configure<TPackageInfo, TPipelineFilter>(config, services, packageHandler);

            return server;
        }

        static void RegisterServices(IServiceCollection services)
        {
            services.UseNetSocketListener();
        }

        static void Main(string[] args)
        {
            var server = CreateSocketServer<LinePackageInfo, LinePipelineFilter>(packageHandler: async (s, p) => 
            {
                await s.SendAsync(Encoding.UTF8.GetBytes(p.Line).AsReadOnlySpan());                
            });
            
            server.Start();

            while (Console.ReadLine().ToLower() != "c")
            {
                continue;
            }
            
            server.Stop();
        }
    }
}
