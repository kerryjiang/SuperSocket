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
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public abstract class TestBase
    {
        protected virtual void RegisterServices(IServiceCollection services)
        {

        }

        protected SuperSocketServer CreateSocketServer<TPackageInfo, TPipelineFilter>(Dictionary<string, string> configDict = null, Action<IAppSession, TPackageInfo> packageHandler = null)
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

            RegisterServices(services);

            var serverOptions = new ServerOptions();
            config.GetSection("serverOptions").Bind(serverOptions);

            Assert.True(server.Configure<TPackageInfo, TPipelineFilter>(serverOptions, services, packageHandler: packageHandler));

            return server;
        }
    }
}
