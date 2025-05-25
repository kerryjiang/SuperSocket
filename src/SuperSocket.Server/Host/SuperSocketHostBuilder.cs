using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Abstractions.Middleware;

namespace SuperSocket.Server.Host
{
    /// <summary>
    /// Provides a builder for configuring and building a SuperSocket host.
    /// </summary>
    /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
    public class SuperSocketHostBuilder<TReceivePackage> : HostBuilderAdapter<SuperSocketHostBuilder<TReceivePackage>>, ISuperSocketHostBuilder<TReceivePackage>, IHostBuilder
    {
        private Func<HostBuilderContext, IConfiguration, IConfiguration> _serverOptionsReader;

        /// <summary>
        /// Gets the list of actions to configure services.
        /// </summary>
        protected List<Action<HostBuilderContext, IServiceCollection>> ConfigureServicesActions { get; private set; } = new List<Action<HostBuilderContext, IServiceCollection>>();

        /// <summary>
        /// Gets the list of actions to configure supplemental services.
        /// </summary>
        protected List<Action<HostBuilderContext, IServiceCollection>> ConfigureSupplementServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperSocketHostBuilder{TReceivePackage}"/> class with the specified host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder to adapt.</param>
        public SuperSocketHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperSocketHostBuilder{TReceivePackage}"/> class with default settings.
        /// </summary>
        public SuperSocketHostBuilder()
            : this(args: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperSocketHostBuilder{TReceivePackage}"/> class with the specified arguments.
        /// </summary>
        /// <param name="args">The command-line arguments for the host builder.</param>
        public SuperSocketHostBuilder(string[] args)
            : base(args)
        {
        }

        private void ConfigureHostBuilder()
        {
            HostBuilder.ConfigureServices((ctx, services) =>
            {
                RegisterBasicServices(ctx, services, services);
            }).ConfigureServices((ctx, services) =>
            {
                foreach (var action in ConfigureServicesActions)
                {
                    action(ctx, services);
                }

                foreach (var action in ConfigureSupplementServicesActions)
                {
                    action(ctx, services);
                }
            }).ConfigureServices((ctx, services) =>
            {
                RegisterDefaultServices(ctx, services, services);
            });
        }

        void IMinimalApiHostBuilder.ConfigureHostBuilder()
        {
            ConfigureHostBuilder();
        }

        /// <summary>
        /// Builds the host for the SuperSocket application.
        /// </summary>
        /// <returns>The built host.</returns>
        public override IHost Build()
        {
            ConfigureHostBuilder();
            return HostBuilder.Build();
        }

        /// <summary>
        /// Configures supplemental services for the host builder.
        /// </summary>
        /// <param name="configureDelegate">The action to configure supplemental services.</param>
        /// <returns>The updated host builder.</returns>
        public ISuperSocketHostBuilder<TReceivePackage> ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureSupplementServicesActions.Add(configureDelegate);
            return this;
        }

        ISuperSocketHostBuilder ISuperSocketHostBuilder.ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureSupplementServices(configureDelegate);
        }

        /// <summary>
        /// Registers basic services required for the SuperSocket host.
        /// </summary>
        /// <param name="builderContext">The host builder context.</param>
        /// <param name="servicesInHost">Services collection from the host.</param>
        /// <param name="services">The service collection to register services into.</param>
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

        /// <summary>
        /// Registers default services required for the SuperSocket host functionality.
        /// </summary>
        /// <param name="builderContext">The host builder context.</param>
        /// <param name="servicesInHost">Services collection from the host.</param>
        /// <param name="services">The service collection to register services into.</param>
        protected virtual void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            // if the package type is StringPackageInfo
            if (typeof(TReceivePackage) == typeof(StringPackageInfo))
            {
                services.TryAdd(ServiceDescriptor.Singleton<IPackageDecoder<StringPackageInfo>, DefaultStringPackageDecoder>());
            }

            services.TryAdd(ServiceDescriptor.Singleton<IPackageEncoder<string>, DefaultStringEncoderForDI>());            
            services.TryAdd(ServiceDescriptor.Singleton<ISessionFactory, DefaultSessionFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<IConnectionListenerFactory, TcpConnectionListenerFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<SocketOptionsSetter>(new SocketOptionsSetter(socket => {})));
            services.TryAdd(ServiceDescriptor.Singleton<IConnectionFactoryBuilder, ConnectionFactoryBuilder>());
            services.TryAdd(ServiceDescriptor.Singleton<IConnectionStreamInitializersFactory, DefaultConnectionStreamInitializersFactory>());

            // if no host service was defined, just use the default one
            if (!CheckIfExistHostedService(services))
            {
                RegisterDefaultHostedService(servicesInHost);
            }
        }

        /// <summary>
        /// Checks if any hosted service of type SuperSocketService is already registered.
        /// </summary>
        /// <param name="services">The service collection to check.</param>
        /// <returns>True if a hosted service is registered, otherwise false.</returns>
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

        /// <summary>
        /// Registers the default hosted service for SuperSocket.
        /// </summary>
        /// <param name="servicesInHost">The service collection to register the hosted service into.</param>
        protected virtual void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            RegisterHostedService<SuperSocketService<TReceivePackage>>(servicesInHost);
        }

        /// <summary>
        /// Registers a specific hosted service for SuperSocket.
        /// </summary>
        /// <typeparam name="THostedService">The type of the hosted service to register.</typeparam>
        /// <param name="servicesInHost">The service collection to register the hosted service into.</param>
        protected virtual void RegisterHostedService<THostedService>(IServiceCollection servicesInHost)
            where THostedService : class, IHostedService
        {
            servicesInHost.AddSingleton<THostedService, THostedService>();
            servicesInHost.AddSingleton<IServerInfo>(s => s.GetService<THostedService>() as IServerInfo);
            servicesInHost.AddHostedService<THostedService>(s => s.GetService<THostedService>());
        }

        /// <summary>
        /// Configures server options for the host builder.
        /// </summary>
        /// <param name="serverOptionsReader">The function to read and configure server options.</param>
        /// <returns>The updated host builder.</returns>
        public ISuperSocketHostBuilder<TReceivePackage> ConfigureServerOptions(Func<HostBuilderContext, IConfiguration, IConfiguration> serverOptionsReader)
        {
            _serverOptionsReader = serverOptionsReader;
            return this;
        }

        ISuperSocketHostBuilder<TReceivePackage> ISuperSocketHostBuilder<TReceivePackage>.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }

        /// <summary>
        /// Configures the services for the host builder.
        /// </summary>
        /// <param name="configureDelegate">The delegate to configure services.</param>
        /// <returns>The updated host builder.</returns>
        public override SuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureServicesActions.Add(configureDelegate);
            return this;
        }

        /// <summary>
        /// Configures the host builder to use a specific pipeline filter.
        /// </summary>
        /// <typeparam name="TPipelineFilter">The type of the pipeline filter to use.</typeparam>
        /// <returns>The configured host builder.</returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : class, IPipelineFilter<TReceivePackage>
        {
            var hasDefaultConstructor = typeof(TPipelineFilter).GetConstructor(Type.EmptyTypes) != null;

            return this.ConfigureServices((ctx, services) =>
                {
                    if (hasDefaultConstructor)
                    {
                        services.AddSingleton(
                            serviceType: typeof(IPipelineFilterFactory<TReceivePackage>),
                            implementationType: typeof(DefaultConstructorPipelineFilterFactory<,>).MakeGenericType(typeof(TReceivePackage), typeof(TPipelineFilter)));
                    }
                    else
                    {
                        services.AddTransient<TPipelineFilter>();
                        services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
                    }

                    services.AddSingleton<IPipelineFilterFactory>(serviceProvider => serviceProvider.GetRequiredService<IPipelineFilterFactory<TReceivePackage>>() as IPipelineFilterFactory);
                });
        }

        /// <summary>
        /// Configures the host builder to use a specific pipeline filter factory.
        /// </summary>
        /// <typeparam name="TPipelineFilterFactory">The type of the pipeline filter factory to use.</typeparam>
        /// <returns>The configured host builder.</returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, TPipelineFilterFactory>();
                services.AddSingleton<IPipelineFilterFactory>(serviceProvider => serviceProvider.GetRequiredService<IPipelineFilterFactory<TReceivePackage>>() as IPipelineFilterFactory);
            });
        }

        /// <summary>
        /// Configures the host builder to use a specific session type.
        /// </summary>
        /// <typeparam name="TSession">The type of session to use.</typeparam>
        /// <returns>The configured host builder.</returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UseSession<TSession>()
            where TSession : IAppSession
        {
            return this.UseSessionFactory<GenericSessionFactory<TSession>>();
        }

        /// <summary>
        /// Configures the host builder to use a specific session factory.
        /// </summary>
        /// <typeparam name="TSessionFactory">The type of the session factory to use.</typeparam>
        /// <returns>The configured host builder.</returns>
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

        /// <summary>
        /// Configures the host builder to use a specific hosted service.
        /// </summary>
        /// <typeparam name="THostedService">The type of the hosted service to use.</typeparam>
        /// <returns>The configured host builder.</returns>
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

        /// <summary>
        /// Configures the host builder to use a specific package decoder.
        /// </summary>
        /// <typeparam name="TPackageDecoder">The type of the package decoder to use.</typeparam>
        /// <returns>The configured host builder.</returns>
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

        /// <summary>
        /// Configures the host builder to use a specific package encoder.
        /// </summary>
        /// <typeparam name="TPackageEncoder">The type of the package encoder to use.</typeparam>
        /// <returns>The configured host builder.</returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UsePackageEncoder<TPackageEncoder>()
            where TPackageEncoder : class, IPackageEncoder<TReceivePackage>
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageEncoder<TReceivePackage>, TPackageEncoder>();
                }
            );
        }

        /// <summary>
        /// Configures the host builder to use a specific middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">The type of the middleware to use.</typeparam>
        /// <returns>The configured host builder.</returns>
        public virtual ISuperSocketHostBuilder<TReceivePackage> UseMiddleware<TMiddleware>()
            where TMiddleware : class, IMiddleware
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
            });
        }

        /// <summary>
        /// Configures the host builder to use a specific package handling scheduler.
        /// </summary>
        /// <typeparam name="TPackageHandlingScheduler">The type of the package handling scheduler to use.</typeparam>
        /// <returns>The configured host builder.</returns>
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

        /// <summary>
        /// Configures the host builder to use a package handling context accessor.
        /// </summary>
        /// <returns>The configured host builder.</returns>
        public ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingContextAccessor()
        {
            return this.ConfigureServices(
                 (hostCtx, services) =>
                 {
                     services.AddSingleton<IPackageHandlingContextAccessor<TReceivePackage>, PackageHandlingContextAccessor<TReceivePackage>>();
                 }
             );
        }

        /// <summary>
        /// Configures the host builder to use GZip compression.
        /// </summary>
        /// <returns>The configured host builder.</returns>
        public ISuperSocketHostBuilder<TReceivePackage> UseGZip()
        {
            return (this as ISuperSocketHostBuilder).UseGZip() as ISuperSocketHostBuilder<TReceivePackage>;
        }
    }

    /// <summary>
    /// Helper class for creating SuperSocketHostBuilder instances.
    /// </summary>
    public static class SuperSocketHostBuilder
    {
        /// <summary>
        /// Creates a new SuperSocketHostBuilder for the specified package type.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of packages to be received.</typeparam>
        /// <returns>A new ISuperSocketHostBuilder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage>()
            where TReceivePackage : class
        {
            return Create<TReceivePackage>(args: null);
        }

        /// <summary>
        /// Creates a new SuperSocketHostBuilder for the specified package type with command line arguments.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of packages to be received.</typeparam>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A new ISuperSocketHostBuilder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage>(string[] args)
        {
            return new SuperSocketHostBuilder<TReceivePackage>(args);
        }

        /// <summary>
        /// Creates a new SuperSocketHostBuilder with a specified package type and pipeline filter.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of packages to be received.</typeparam>
        /// <typeparam name="TPipelineFilter">The type of pipeline filter to use.</typeparam>
        /// <returns>A new ISuperSocketHostBuilder with the specified pipeline filter.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TPipelineFilter : class, IPipelineFilter<TReceivePackage>
        {
            return Create<TReceivePackage, TPipelineFilter>(args: null);
        }

        /// <summary>
        /// Creates a new SuperSocketHostBuilder with a specified package type, pipeline filter, and command line arguments.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of packages to be received.</typeparam>
        /// <typeparam name="TPipelineFilter">The type of pipeline filter to use.</typeparam>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A new ISuperSocketHostBuilder with the specified pipeline filter.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>(string[] args)
            where TPipelineFilter : class, IPipelineFilter<TReceivePackage>
        {
            return new SuperSocketHostBuilder<TReceivePackage>(args)
                .UsePipelineFilter<TPipelineFilter>();
        }
    }
}
