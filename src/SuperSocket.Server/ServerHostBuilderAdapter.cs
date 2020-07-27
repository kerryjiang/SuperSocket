using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Server
{
    public interface IServerHostBuilderAdapter
    {
        void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices);

        void ConfigureServiceProvider(IServiceProvider hostServiceProvider);
    }

    public class ServerHostBuilderAdapter<TReceivePackage> : SuperSocketHostBuilder<TReceivePackage>, IServerHostBuilderAdapter
    {
        private IHostBuilder _hostBuilder;

        private IServiceCollection _currentServices = new ServiceCollection();
        
        private IServiceProvider _serviceProvider;

        private IServiceProvider _hostServiceProvider;

        public ServerHostBuilderAdapter(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        void IServerHostBuilderAdapter.ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            ConfigureServer(context, hostServices);
        }

        protected void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            var services = _currentServices;

            CopyGlobalServices(hostServices, services);

            RegisterBasicServices(context, hostServices, services);

            foreach (var configureServicesAction in ConfigureServicesActions)
            {
                configureServicesAction(context, services);
            }

            RegisterDefaultServices(context, hostServices, services);

            var serviceFactory = new DefaultServiceProviderFactory();
            var containerBuilder = serviceFactory.CreateBuilder(services);

            _serviceProvider = serviceFactory.CreateServiceProvider(containerBuilder);
        }

        private void CopyGlobalServices(IServiceCollection hostServices, IServiceCollection services)
        {
            foreach (var sd in hostServices)
            {
                if (sd.ServiceType == typeof(IHostedService))
                    continue;
                
                CopyGlobalServiceDescriptor(hostServices, services, sd);
            }
        }

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
                if (!sd.ServiceType.IsGenericTypeDefinition)
                    services.Add(new ServiceDescriptor(sd.ServiceType, (sp) => _hostServiceProvider.GetService(sd.ServiceType), sd.Lifetime));
                else
                    services.Add(sd);
            }            
        }

        private object GetServiceFromHost(Func<IServiceProvider, object> factory)
        {
            return factory(_hostServiceProvider);
        }

        void IServerHostBuilderAdapter.ConfigureServiceProvider(IServiceProvider hostServiceProvider)
        {
            _hostServiceProvider = hostServiceProvider;
        }
   
        protected void RegisterHostedService<THostedService>()
            where THostedService : class, IHostedService
        {
            base.HostBuilder.ConfigureServices((context, services) =>
            {
                RegisterHostedService<THostedService>(services);
            });
        }

        private void RegisterHostedService<THostedService>(IServiceCollection servicesInHost)
            where THostedService : class, IHostedService
        {
            _currentServices.AddSingleton<IHostedService, THostedService>();
            servicesInHost.AddHostedService<THostedService>(s => GetHostedService<THostedService>());
        }

        protected override void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            RegisterHostedService<SuperSocketService<TReceivePackage>>(servicesInHost);
        }

        private THostedService GetHostedService<THostedService>()
        {
            return (THostedService)_serviceProvider.GetService<IHostedService>();
        }

        public override ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
        {
            RegisterHostedService<THostedService>();
            return this;
        }

        public override IHost Build()
        {
            throw new NotSupportedException();
        }

        public override SuperSocketHostBuilder<TReceivePackage> UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            throw new NotSupportedException();
        }

        public override SuperSocketHostBuilder<TReceivePackage> UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            throw new NotSupportedException();
        }        
    }
}