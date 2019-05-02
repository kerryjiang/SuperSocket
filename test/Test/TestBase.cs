using System;
using System.Collections.Generic;
using System.Text;
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

        protected static readonly Encoding Utf8Encoding = new UTF8Encoding();

        protected TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }


        protected IHostBuilder<TPackageInfo> CreateSocketServerBuilder<TPackageInfo>(Func<object, IPipelineFilter<TPackageInfo>> filterFactory)
            where TPackageInfo : class
        {
            return Configure(SuperSocketHostBuilder.Create<TPackageInfo>(filterFactory)) as IHostBuilder<TPackageInfo>;
        }

        protected IHostBuilder<TPackageInfo> CreateSocketServerBuilder<TPackageInfo, TPipelineFilter>()
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            return Configure(SuperSocketHostBuilder.Create<TPackageInfo, TPipelineFilter>()) as IHostBuilder<TPackageInfo>;
        }

        protected IHostBuilder Configure(IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureAppConfiguration((hostCtx, configApp) =>
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
        }
    }
}
