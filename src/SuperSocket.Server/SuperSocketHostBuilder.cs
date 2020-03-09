using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket
{
    public class SuperSocketHostBuilder<TReceivePackage> : IHostBuilder<TReceivePackage>
        where TReceivePackage : class
    {

        private HostBuilder _hostBuilder;

        public SuperSocketHostBuilder()
        {
            _hostBuilder = new HostBuilder();
        }

        public IDictionary<object, object> Properties => _hostBuilder.Properties;

        public IHost Build()
        {
            return _hostBuilder.ConfigureServices((ctx, services) => 
            {
                services.TryAdd(new ServiceDescriptor(typeof(IPackageEncoder<string>), typeof(DefaultStringEncoderForDI), ServiceLifetime.Singleton));
            }).Build();
        }

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureAppConfiguration(configureDelegate);
            return this;
        }

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureContainer<TContainerBuilder>(configureDelegate);
            return this;
        }

        public IHostBuilder<TReceivePackage> ConfigureDefaults()
        {
            var hostBuilder = this.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                });

            return hostBuilder.ConfigureServices((hostCtx, services) =>
                {
                    // if the package type is StringPackageInfo
                    if (typeof(TReceivePackage) == typeof(StringPackageInfo))
                    {
                        services.TryAdd(new ServiceDescriptor(typeof(IPackageDecoder<StringPackageInfo>), typeof(DefaultStringPackageDecoder), ServiceLifetime.Singleton));
                    }

                    services.AddOptions();
                    services.Configure<ServerOptions>(hostCtx.Configuration.GetSection("serverOptions"));
                }) as IHostBuilder<TReceivePackage>;
        }

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _hostBuilder.ConfigureServices(configureDelegate);
            return this;
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _hostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this;
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _hostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this;
        }
    }

    public static class SuperSocketHostBuilder
    {
        public static IHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .ConfigureDefaults()
                .UseSuperSocket<TReceivePackage, TPipelineFilter>() as IHostBuilder<TReceivePackage>;
        }

        public static IHostBuilder<TReceivePackage> Create<TReceivePackage>(Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .ConfigureDefaults()
                .UseSuperSocket<TReceivePackage>(filterFactory) as IHostBuilder<TReceivePackage>;
        }
        
        public static IHostBuilder<TReceivePackage> Create<TReceivePackage, TSuperSocketService, TPipelineFilter>()
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
            where TSuperSocketService : SuperSocketService<TReceivePackage>
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .ConfigureDefaults()
                .UseSuperSocket<TReceivePackage, TPipelineFilter, TSuperSocketService>() as IHostBuilder<TReceivePackage>;
        }
    }
}
