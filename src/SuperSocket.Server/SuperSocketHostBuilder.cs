using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SuperSocketHostBuilder<TReceivePackage> : ISuperSocketHostBuilder<TReceivePackage>, IHostBuilder
    {

        private HostBuilder _hostBuilder;

        public SuperSocketHostBuilder()
        {
            _hostBuilder = new HostBuilder();
        }

        public IDictionary<object, object> Properties => _hostBuilder.Properties;

        public virtual IHost Build()
        {
            return _hostBuilder.ConfigureServices((ctx, services) => 
            {
                // if the package type is StringPackageInfo
                if (typeof(TReceivePackage) == typeof(StringPackageInfo))
                {
                    services.TryAdd(ServiceDescriptor.Singleton<IPackageDecoder<StringPackageInfo>, DefaultStringPackageDecoder>());
                }

                services.TryAdd(ServiceDescriptor.Singleton<IPackageEncoder<string>, DefaultStringEncoderForDI>());

                // if no host service was defined, just use the default one
                if (!services.Any(sd => sd.ServiceType == typeof(IHostedService)
                    && typeof(SuperSocketService<TReceivePackage>).IsAssignableFrom(sd.ImplementationType)))
                {
                    RegisterDefaultHostedService(services);
                }
            }).Build();
        }

        protected virtual void RegisterDefaultHostedService(IServiceCollection services)
        {
            services.AddHostedService<SuperSocketService<TReceivePackage>>();
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

        public SuperSocketHostBuilder<TReceivePackage> ConfigureDefaults()
        {
            var hostBuilder = this.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                });

            return this.ConfigureServices((hostCtx, services) =>
                {
                    services.AddOptions();
                    services.Configure<ServerOptions>(hostCtx.Configuration.GetSection("serverOptions"));
                });
        }

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }

        public SuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
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

        public virtual SuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
            });
        }

        public virtual SuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, TPipelineFilterFactory>();
            });
        }

        public virtual SuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
            where THostedService : SuperSocketService<TReceivePackage>
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddHostedService<THostedService>();
            });
        }
        

        public virtual SuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TPackageDecoder>()
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageDecoder<TReceivePackage>, TPackageDecoder>();
                }
            );
        }

        public virtual SuperSocketHostBuilder<TReceivePackage> ConfigureErrorHandler(Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler)
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }

        public virtual SuperSocketHostBuilder<TReceivePackage> UsePackageHandler(Func<IAppSession, TReceivePackage, ValueTask> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    if (packageHandler != null)
                        services.AddSingleton<IPackageHandler<TReceivePackage>>(new DelegatePackageHandler<TReceivePackage>(packageHandler));

                    if (errorHandler != null)
                        services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }

        public SuperSocketHostBuilder<TReceivePackage> UsePackageHandlingScheduler<TPackageHandlingScheduler>()
            where TPackageHandlingScheduler : class, IPackageHandlingScheduler<TReceivePackage>
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageHandlingScheduler<TReceivePackage>, TPackageHandlingScheduler>();
                }
            );
        }
    }

    public static class SuperSocketHostBuilder
    {
        public static SuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage>()
            where TReceivePackage : class
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .ConfigureDefaults();
        }
        
        public static SuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .ConfigureDefaults()
                .UsePipelineFilter<TPipelineFilter>();
        }
    }
}
