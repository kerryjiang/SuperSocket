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
    public class SuperSocketHostBuilder<TReceivePackage> : HostBuilderAdapter<SuperSocketHostBuilder<TReceivePackage>>, ISuperSocketHostBuilder<TReceivePackage>, IHostBuilder
    {
        private bool _appConfigSet = false;

        private Func<HostBuilderContext, IConfiguration, IConfiguration> _serverOptionsReader;

        protected List<Action<HostBuilderContext, IServiceCollection>> ConfigureServicesActions { get; private set; } = new List<Action<HostBuilderContext, IServiceCollection>>();

        public SuperSocketHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {

        }

        public SuperSocketHostBuilder()
            : base()
        {

        }

        public override IHost Build()
        {
            return HostBuilder.ConfigureServices((ctx, services) => 
            {
                RegisterBasicServices(ctx, services, services);
            }).ConfigureServices((ctx, services) => 
            {
                foreach (var action in ConfigureServicesActions)
                {
                    action(ctx, services);
                }
            }).ConfigureServices((ctx, services) => 
            {
                RegisterDefaultServices(ctx, services, services);
            }).Build();
        }


        protected void RegisterBasicServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            if (!_appConfigSet)
                this.ConfigureAppConfiguration(SuperSocketHostBuilder.ConfigureAppConfiguration);

            var serverOptionReader = _serverOptionsReader;

            if (serverOptionReader == null)
            {
                serverOptionReader = (ctx, config) =>
                {
                    return config;
                };
            }

            services.AddOptions();
            services.Configure<ServerOptions>(serverOptionReader(builderContext, builderContext.Configuration.GetSection("serverOptions")));
        }

        protected void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            // if the package type is StringPackageInfo
            if (typeof(TReceivePackage) == typeof(StringPackageInfo))
            {
                services.TryAdd(ServiceDescriptor.Singleton<IPackageDecoder<StringPackageInfo>, DefaultStringPackageDecoder>());
            }

            services.TryAdd(ServiceDescriptor.Singleton<IPackageEncoder<string>, DefaultStringEncoderForDI>());

            // if no host service was defined, just use the default one
            if (!CheckIfExistHostedService(services))
            {
                RegisterDefaultHostedService(servicesInHost);
            }
        }

        protected bool CheckIfExistHostedService(IServiceCollection services)
        {
            return services.Any(sd => sd.ServiceType == typeof(IHostedService)
                && typeof(SuperSocketService<TReceivePackage>).IsAssignableFrom(sd.ImplementationType));
        }

        protected virtual void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            servicesInHost.AddHostedService<SuperSocketService<TReceivePackage>>();
        }

        public SuperSocketHostBuilder<TReceivePackage> ConfigureServerOptions(Func<HostBuilderContext, IConfiguration, IConfiguration> serverOptionsReader)
        {
             _serverOptionsReader = serverOptionsReader;
             return this;
        }

        public override SuperSocketHostBuilder<TReceivePackage> ConfigureAppConfiguration(Action<HostBuilderContext,IConfigurationBuilder> configDelegate)
        {
            _appConfigSet = true;
            return base.ConfigureAppConfiguration(configDelegate);
        }

        public override SuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureServicesActions.Add(configureDelegate);
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
            return new SuperSocketHostBuilder<TReceivePackage>();
        }
        
        public static SuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .UsePipelineFilter<TPipelineFilter>();
        }

        internal static void ConfigureAppConfiguration(HostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        }
    }
}
