using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.Server.Host
{
    /// <summary>
    /// Represents a server host builder adapter for SuperSocket.
    /// </summary>
    /// <typeparam name="TReceivePackage">The type of the received package.</typeparam>
    public class ServerHostBuilderAdapter<TReceivePackage> : SuperSocketHostBuilder<TReceivePackage>, IServerHostBuilderAdapter
    {
        private IHostBuilder _hostBuilder;

        private IServiceCollection _currentServices = new ServiceCollection();
        
        private IServiceProvider _serviceProvider;

        private IServiceProvider _hostServiceProvider;

        private Func<HostBuilderContext, IServiceCollection, IServiceProvider> _serviceProviderBuilder = null;

        private List<IConfigureContainerAdapter> _configureContainerActions = new List<IConfigureContainerAdapter>();

        private string _serverName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerHostBuilderAdapter{TReceivePackage}"/> class.
        /// </summary>
        /// <param name="hostBuilder">The host builder to adapt.</param>
        /// <param name="serverName">The optional server name for keyed service registration.</param>
        public ServerHostBuilderAdapter(IHostBuilder hostBuilder, string serverName = null)
            : base(hostBuilder)
        {
            _hostBuilder = hostBuilder;
            _serverName = serverName;
        }

        /// <summary>
        /// Configures the server with the specified host builder context and service collection.
        /// </summary>
        /// <param name="context">The host builder context.</param>
        /// <param name="hostServices">The service collection for the host.</param>
        void IServerHostBuilderAdapter.ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            ConfigureServer(context, hostServices);
        }

        /// <summary>
        /// Configures the server with the specified host builder context and service collection.
        /// </summary>
        /// <param name="context">The host builder context.</param>
        /// <param name="hostServices">The service collection for the host.</param>
        protected void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            var services = _currentServices;

            // If a server name is provided, configure server options and register the server name
            if (!string.IsNullOrEmpty(_serverName))
            {
                this.ConfigureServerOptions((ctx, options) =>
                {
                    return options.GetSection(_serverName);
                });

                services.PostConfigure<ServerOptions>(options =>
                {
                    options.Name = _serverName;
                });
            }

            CopyGlobalServices(hostServices, services);

            RegisterBasicServices(context, hostServices, services);

            foreach (var configureServicesAction in ConfigureServicesActions)
            {
                configureServicesAction(context, services);
            }

            foreach (var configureServicesAction in ConfigureSupplementServicesActions)
            {
                configureServicesAction(context, services);
            }

            RegisterDefaultServices(context, hostServices, services);

            if (_serviceProviderBuilder == null)
            {
                var serviceFactory = new DefaultServiceProviderFactory();
                var containerBuilder = serviceFactory.CreateBuilder(services);
                ConfigureContainerBuilder(context, containerBuilder);
                _serviceProvider = serviceFactory.CreateServiceProvider(containerBuilder);
            }
            else
            {
                _serviceProvider = _serviceProviderBuilder(context, services);
            }
        }

        /// <summary>
        /// Configures the container builder with the specified context and container builder.
        /// </summary>
        /// <param name="context">The host builder context.</param>
        /// <param name="containerBuilder">The container builder to configure.</param>
        private void ConfigureContainerBuilder(HostBuilderContext context, object containerBuilder)
        {
            foreach (IConfigureContainerAdapter containerAction in _configureContainerActions)
                containerAction.ConfigureContainer(context, containerBuilder);
        }

        /// <summary>
        /// Copies global services from the host services to the specified service collection.
        /// </summary>
        /// <param name="hostServices">The host services.</param>
        /// <param name="services">The target service collection.</param>
        private void CopyGlobalServices(IServiceCollection hostServices, IServiceCollection services)
        {
            foreach (var sd in hostServices)
            {
                if (sd.ServiceType == typeof(IHostedService))
                    continue;
                
                CopyGlobalServiceDescriptor(hostServices, services, sd);
            }
        }

        /// <summary>
        /// Copies a global service descriptor from the host services to the specified service collection.
        /// </summary>
        /// <param name="hostServices">The host services.</param>
        /// <param name="services">The target service collection.</param>
        /// <param name="sd">The service descriptor to copy.</param>
        private void CopyGlobalServiceDescriptor(IServiceCollection hostServices, IServiceCollection services, ServiceDescriptor sd)
        {
            if (sd.ImplementationInstance != null)
            {
                services.Add(new ServiceDescriptor(sd.ServiceType, sd.ImplementationInstance));
            }
            else if (sd.ImplementationFactory != null)
            {
                services.Add(new ServiceDescriptor(sd.ServiceType, (sp) => GetServiceFromHost(sd.ImplementationFactory), sd.Lifetime));
            }
            else if (sd.ImplementationType != null)
            {
                if (!sd.ServiceType.IsGenericTypeDefinition && sd.Lifetime == ServiceLifetime.Singleton)
                    services.Add(new ServiceDescriptor(sd.ServiceType, (sp) => _hostServiceProvider.GetService(sd.ServiceType), ServiceLifetime.Singleton));
                else
                    services.Add(sd);
            }
        }

        /// <summary>
        /// Gets a service from the host using the specified factory.
        /// </summary>
        /// <param name="factory">The factory to create the service.</param>
        /// <returns>The created service.</returns>
        private object GetServiceFromHost(Func<IServiceProvider, object> factory)
        {
            return factory(_hostServiceProvider);
        }

        /// <summary>
        /// Configures the service provider with the specified host service provider.
        /// </summary>
        /// <param name="hostServiceProvider">The host service provider.</param>
        void IServerHostBuilderAdapter.ConfigureServiceProvider(IServiceProvider hostServiceProvider)
        {
            _hostServiceProvider = hostServiceProvider;
        }
   
        /// <summary>
        /// Registers a hosted service of the specified type.
        /// </summary>
        /// <typeparam name="THostedService">The type of the hosted service.</typeparam>
        protected void RegisterHostedService<THostedService>()
            where THostedService : class, IHostedService
        {
            base.HostBuilder.ConfigureServices((context, services) =>
            {
                RegisterHostedService<THostedService>(services);
            });
        }

        /// <summary>
        /// Registers a hosted service of the specified type in the provided service collection.
        /// </summary>
        /// <typeparam name="THostedService">The type of the hosted service.</typeparam>
        /// <param name="servicesInHost">The service collection to register the hosted service in.</param>
        protected override void RegisterHostedService<THostedService>(IServiceCollection servicesInHost)
        {
            _currentServices.AddSingleton<IHostedService, THostedService>();
            _currentServices.AddSingleton<IServerInfo>(s => s.GetService<IHostedService>() as IServerInfo);
            
            if (_serverName != null)
            {
                // Register as keyed service
                servicesInHost.AddKeyedSingleton<IHostedService>(_serverName, (s, k) => GetHostedService<THostedService>());
                servicesInHost.AddKeyedSingleton<THostedService>(_serverName, (s, k) => GetHostedService<THostedService>());
                servicesInHost.AddKeyedSingleton<IServerInfo>(_serverName, (s, k) => GetHostedService<THostedService>() as IServerInfo);
                servicesInHost.AddSingleton<IHostedService>(_ => GetHostedService<THostedService>());
                servicesInHost.AddSingleton<THostedService>(_ => GetHostedService<THostedService>());
            }
            else
            {
                servicesInHost.AddHostedService<THostedService>(s => GetHostedService<THostedService>());
            }
        }

        /// <summary>
        /// Registers the default hosted service in the provided service collection.
        /// </summary>
        /// <param name="servicesInHost">The service collection to register the default hosted service in.</param>
        protected override void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            RegisterHostedService<SuperSocketService<TReceivePackage>>(servicesInHost);
        }

        /// <summary>
        /// Gets the hosted service of the specified type.
        /// </summary>
        /// <typeparam name="THostedService">The type of the hosted service.</typeparam>
        /// <returns>The hosted service.</returns>
        private THostedService GetHostedService<THostedService>()
        {
            return (THostedService)_serviceProvider.GetService<IHostedService>();
        }

        /// <summary>
        /// Configures the host builder to use a hosted service of the specified type.
        /// </summary>
        /// <typeparam name="THostedService">The type of the hosted service.</typeparam>
        /// <returns>The current instance of <see cref="ISuperSocketHostBuilder{TReceivePackage}"/>.</returns>
        public override ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
        {
            RegisterHostedService<THostedService>();
            return this;
        }

        /// <summary>
        /// Builds the host. This method is not supported.
        /// </summary>
        /// <returns>Throws a <see cref="NotSupportedException"/>.</returns>
        public override IHost Build()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Configures the container with the specified delegate.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
        /// <param name="configureDelegate">The delegate to configure the container.</param>
        /// <returns>The current instance of <see cref="SuperSocketHostBuilder{TReceivePackage}"/>.</returns>
        public override SuperSocketHostBuilder<TReceivePackage> ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate));
            return this;
        }

        /// <summary>
        /// Configures the host builder to use the specified service provider factory.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
        /// <param name="factory">The service provider factory to use.</param>
        /// <returns>The current instance of <see cref="SuperSocketHostBuilder{TReceivePackage}"/>.</returns>
        public override SuperSocketHostBuilder<TReceivePackage> UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _serviceProviderBuilder = (context, services) =>
            {
                var containerBuilder = factory.CreateBuilder(services);
                ConfigureContainerBuilder(context, containerBuilder);
                return factory.CreateServiceProvider(containerBuilder);                
            };
            return this;
        }

        /// <summary>
        /// Configures the host builder to use the specified service provider factory.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
        /// <param name="factory">The factory function to create the service provider factory.</param>
        /// <returns>The current instance of <see cref="SuperSocketHostBuilder{TReceivePackage}"/>.</returns>
        public override SuperSocketHostBuilder<TReceivePackage> UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _serviceProviderBuilder = (context, services) =>
            {
                var serviceProviderFactory = factory(context);
                var containerBuilder = serviceProviderFactory.CreateBuilder(services);
                ConfigureContainerBuilder(context, containerBuilder);
                return serviceProviderFactory.CreateServiceProvider(containerBuilder);                
            };
            return this;
        }
    }
}