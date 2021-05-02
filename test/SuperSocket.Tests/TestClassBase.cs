using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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

namespace SuperSocket.Tests
{
    public abstract class TestClassBase
    {
        protected readonly ITestOutputHelper OutputHelper;
        protected static readonly Encoding Utf8Encoding = new UTF8Encoding();
        protected readonly static int DefaultServerPort = 4040;
        protected readonly static int AlternativeServerPort = 4041;

        protected IPEndPoint GetDefaultServerEndPoint()
        {
            return new IPEndPoint(IPAddress.Loopback, DefaultServerPort);
        }

        protected IPEndPoint GetAlternativeServerEndPoint()
        {
            return new IPEndPoint(IPAddress.Loopback, AlternativeServerPort);
        }

        protected static ILoggerFactory DefaultLoggerFactory { get; } = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });

        protected TestClassBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }


        protected SuperSocketHostBuilder<TPackageInfo> CreateSocketServerBuilder<TPackageInfo>(Func<object, IPipelineFilter<TPackageInfo>> filterFactory, IHostConfigurator configurator = null)
            where TPackageInfo : class
        {
            var hostBuilder = SuperSocketHostBuilder.Create<TPackageInfo>();
            hostBuilder.UsePipelineFilterFactory(filterFactory);
            return Configure(hostBuilder, configurator) as SuperSocketHostBuilder<TPackageInfo>;
        }

        protected SuperSocketHostBuilder<TPackageInfo> CreateSocketServerBuilder<TPackageInfo, TPipelineFilter>(IHostConfigurator configurator = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            var hostBuilder = SuperSocketHostBuilder.Create<TPackageInfo>();
            hostBuilder.UsePipelineFilter<TPipelineFilter>();
            return Configure(hostBuilder, configurator) as SuperSocketHostBuilder<TPackageInfo>;
        }

        protected T CreateObject<T>(Type type)
        {
            return (T)ActivatorUtilities.CreateFactory(type, new Type[0]).Invoke(null, null);
        }

        protected Socket CreateClient(IHostConfigurator hostConfigurator)
        {
            return hostConfigurator.CreateClient();
        }

        protected ISuperSocketHostBuilder Configure(ISuperSocketHostBuilder hostBuilder, IHostConfigurator configurator = null)
        {
            var builder = hostBuilder.ConfigureAppConfiguration((hostCtx, configApp) =>
                {
                    configApp.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "serverOptions:name", "TestServer" },
                        { "serverOptions:listeners:0:ip", "Any" },
                        { "serverOptions:listeners:0:backLog", "100" },
                        { "serverOptions:listeners:0:port", DefaultServerPort.ToString() }
                    });
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                    loggingBuilder.AddDebug();
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    ConfigureServices(hostCtx, services);
                }) as ISuperSocketHostBuilder;
            
            configurator?.Configure(builder);
            
            return builder;
        }
    }
}
