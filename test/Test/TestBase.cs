using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public abstract class TestBase
    {
        protected readonly ITestOutputHelper OutputHelper;

        protected TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }


        protected IHostBuilder CreateSocketServerBuilder<TPackageInfo>(Func<object, IPipelineFilter<TPackageInfo>> filterFactory)
            where TPackageInfo : class
        {
            return CreateSocketServerBuilderBase().UseSuperSocket<TPackageInfo>(filterFactory);
        }

        protected IHostBuilder CreateSocketServerBuilder<TPackageInfo, TPipelineFilter>()
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            return CreateSocketServerBuilderBase().UseSuperSocket<TPackageInfo, TPipelineFilter>();
        }

        protected IHostBuilder CreateSocketServerBuilderBase()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostCtx, configApp) =>
                {
                    configApp.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "serverOptions:name", "TestServer" },
                        { "serverOptions:listeners:0:ip", "Any" },
                        { "serverOptions:listeners:0:port", "4040" }
                    });
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddOptions();
                    services.Configure<ServerOptions>(hostCtx.Configuration.GetSection("serverOptions"));
                    ConfigureServices(hostCtx, services);
                });

            return hostBuilder;
        }
    }
}
