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
        private Func<HostBuilderContext, IConfiguration, IConfiguration> _serverOptionsReader;

        protected List<Action<HostBuilderContext, IServiceCollection>> ConfigureServicesActions { get; private set; } = new List<Action<HostBuilderContext, IServiceCollection>>();
        
        protected Action<HostBuilderContext, IServiceCollection> Validator { get; set; }
        
        public SuperSocketHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {

        }

        public SuperSocketHostBuilder()
            : this(args: null)
        {

        }

        public SuperSocketHostBuilder(string[] args)
            : base(args)
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

                Validator?.Invoke(ctx, services);                
            }).ConfigureServices((ctx, services) => 
            {
                RegisterDefaultServices(ctx, services, services);
            }).Build();
        }

        protected virtual void RegisterBasicServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            var serverOptionReader = _serverOptionsReader;

            if (serverOptionReader == null)
            {
                serverOptionReader = (ctx, config) =>
                {
                    return config;
                };
            }

            services.AddOptions();

            var config = builderContext.Configuration.GetSection("serverOptions");
            var serverConfig = serverOptionReader(builderContext, config);
                                                
            services.Configure<ServerOptions>(serverConfig);
        }

        protected virtual void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
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

        protected virtual bool CheckIfExistHostedService(IServiceCollection services)
        {
            return services.Any(s => s.ServiceType == typeof(IHostedService)
                && typeof(SuperSocketService<TReceivePackage>).IsAssignableFrom(GetImplementationType(s)));
        }

        private Type GetImplementationType(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationType != null)
                return serviceDescriptor.ImplementationType;
            
            if (serviceDescriptor.ImplementationInstance != null)
                return serviceDescriptor.ImplementationInstance.GetType();
            
            if (serviceDescriptor.ImplementationFactory != null)
            {
                var typeArguments = serviceDescriptor.ImplementationFactory.GetType().GenericTypeArguments;

                if (typeArguments.Length == 2)
                    return typeArguments[1];
            }

            return null;
        }

        protected virtual void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            RegisterHostedService<SuperSocketService<TReceivePackage>>(servicesInHost);
        }

        protected virtual void RegisterHostedService<THostedService>(IServiceCollection servicesInHost)
            where THostedService : class, IHostedService
        {
            servicesInHost.AddSingleton<THostedService, THostedService>();
            servicesInHost.AddSingleton<IServerInfo>(s => s.GetService<THostedService>() as IServerInfo);
            servicesInHost.AddHostedService<THostedService>(s => s.GetService<THostedService>());
        }

        public ISuperSocketHostBuilder<TReceivePackage> ConfigureServerOptions(Func<HostBuilderContext, IConfiguration, IConfiguration> serverOptionsReader)
        {
             _serverOptionsReader = serverOptionsReader;
             return this;
        }

        ISuperSocketHostBuilder<TReceivePackage> ISuperSocketHostBuilder<TReceivePackage>.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }

        public override SuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureServicesActions.Add(configureDelegate);
            return this;
        }        

        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
            });
        }

        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, TPipelineFilterFactory>();
            });
        }

        public virtual ISuperSocketHostBuilder<TReceivePackage> UseSession<TSession>()
            where TSession : IAppSession
        {
            return this.UseSessionFactory<GenericSessionFactory<TSession>>();
        }

        public virtual ISuperSocketHostBuilder<TReceivePackage> UseSessionFactory<TSessionFactory>()
            where TSessionFactory : class, ISessionFactory
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<ISessionFactory, TSessionFactory>();
                }
            );
        }

        public virtual ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
            where THostedService : class, IHostedService
        {
            if (!typeof(SuperSocketService<TReceivePackage>).IsAssignableFrom(typeof(THostedService)))
            {
                throw new ArgumentException($"The type parameter should be subclass of {nameof(SuperSocketService<TReceivePackage>)}", nameof(THostedService));
            }

            return this.ConfigureServices((ctx, services) =>
            {
                RegisterHostedService<THostedService>(services);
            });
        }
        

        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TPackageDecoder>()
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageDecoder<TReceivePackage>, TPackageDecoder>();
                }
            );
        }

        public virtual ISuperSocketHostBuilder<TReceivePackage> UseMiddleware<TMiddleware>()
            where TMiddleware : class, IMiddleware
        {
            return this.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
            });
        }

        public ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingScheduler<TPackageHandlingScheduler>()
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
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage>()
            where TReceivePackage : class
        {
            return Create<TReceivePackage>(args: null);
        }

        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage>(string[] args)
        {
            return new SuperSocketHostBuilder<TReceivePackage>(args);
        }

        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return Create<TReceivePackage, TPipelineFilter>(args: null);
        }
        
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>(string[] args)
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TReceivePackage>(args)
                .UsePipelineFilter<TPipelineFilter>();
        }
    }
}
