using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Buffers;
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
        static SuperSocketServer CreateSocketServer<TPackageInfo, TPipelineFilter>(Dictionary<string, string> configDict = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            if (configDict == null)
            {
                configDict = new Dictionary<string, string>
                {
                    { "serverOptions:name", "TestServer" },
                    { "serverOptions:listeners:0:ip", "Any" },
                    { "serverOptions:listeners:0:port", "4040" }
                };
            }

            var server = new SuperSocketServer();

            var services = new ServiceCollection();

            var builder = new ConfigurationBuilder().AddInMemoryCollection(configDict);
            var config = builder.Build();
            
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            var serverOptions = new ServerOptions();
            config.GetSection("serverOptions").Bind(serverOptions);

            server.Configure<TPackageInfo, TPipelineFilter>(serverOptions, services, packageHandler: packageHandler);

            return server;
        }

        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            var server = CreateSocketServer<TextPackageInfo, LinePipelineFilter>(packageHandler: async (s, p) => 
            {
                await s.Channel.SendAsync(Encoding.UTF8.GetBytes(p.Text).AsMemory());                
            });
            
            await server.StartAsync();

            Console.WriteLine("The server is started.");

            while (Console.ReadLine().ToLower() != "c")
            {
                continue;
            }
            
            await server.StopAsync();
        }
    }
}
