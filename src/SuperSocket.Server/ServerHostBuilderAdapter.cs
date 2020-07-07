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

    public class ServerHostBuilderAdapter<TReceivePackage> : SuperSocketHostBuilder<TReceivePackage>, IServerHostBuilderAdapter, IServiceProviderAccessor
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

            services.AddSingleton<IServiceProviderAccessor>(this);

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
            CopyGlobalServiceType<IHostEnvironment>(hostServices, services);
            CopyGlobalServiceType<HostBuilderContext>(hostServices, services);
            CopyGlobalServiceType<IConfiguration>(hostServices, services);
            CopyGlobalServiceType<IHostApplicationLifetime>(hostServices, services);
            CopyGlobalServiceType<IHostLifetime>(hostServices, services);
            CopyGlobalServiceType<IHost>(hostServices, services);
            CopyGlobalServiceType<ILoggerFactory>(hostServices, services);
        }

        private void CopyGlobalServiceType<TService>(IServiceCollection hostServices, IServiceCollection services)
        {
            var sd = hostServices.FirstOrDefault(t => t.ServiceType == typeof(TService));

            if (sd == null)
                return;

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
                services.Add(new ServiceDescriptor(sd.ServiceType, (sp) => _hostServiceProvider.GetService(sd.ServiceType), sd.Lifetime));
            }            
        }

        private object GetServiceFromHost(Func<IServiceProvider, object> factory)
        {
            return factory(_hostServiceProvider);
        }

        IServiceProvider IServiceProviderAccessor.ServiceProvider
        {
            get { return _serviceProvider; }
        }

        void IServerHostBuilderAdapter.ConfigureServiceProvider(IServiceProvider hostServiceProvider)
        {
            _hostServiceProvider = hostServiceProvider;
            _serviceProvider = new MultipleServerHostServiceProvider(_serviceProvider, hostServiceProvider);
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