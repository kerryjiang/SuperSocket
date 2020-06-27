using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    interface IServerHostBuilderAdapter
    {
        void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices);
    }

    class ServerHostBuilderAdapter<TReceivePackage> : SuperSocketHostBuilder<TReceivePackage>, IServerHostBuilderAdapter
    {
        private IHostBuilder _hostBuilder;

        private IServiceCollection _currentServices = new ServiceCollection();

        
        private IServiceProvider _serviceProvider;

        internal ServerHostBuilderAdapter(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            _hostBuilder = hostBuilder;
            
        }

        private void CopyGlobalServices(IServiceCollection hostServices, IServiceCollection services)
        {
            CopyGlobalServiceType<IHostEnvironment>(hostServices, services);

            #pragma warning disable CS0618
            CopyGlobalServiceType<IHostingEnvironment>(hostServices, services);
            #pragma warning restore CS0618

            CopyGlobalServiceType<HostBuilderContext>(hostServices, services);
            CopyGlobalServiceType<IConfiguration>(hostServices, services);

            CopyGlobalServiceType<IHostApplicationLifetime>(hostServices, services);
            CopyGlobalServiceType<IHostLifetime>(hostServices, services);
            CopyGlobalServiceType<IHost>(hostServices, services);
        }

        private void CopyGlobalServiceType<TService>(IServiceCollection hostServices, IServiceCollection services)
        {
            var sd = services.FirstOrDefault(t => t.ServiceType == typeof(TService));

            if (sd != null)
                services.Add(sd);
        }

        void IServerHostBuilderAdapter.ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            ConfigureServer(context, hostServices);
        }

        protected void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices)
        {
            var services = _currentServices;

            CopyGlobalServices(hostServices, services);

            services.AddOptions();
            services.AddLogging();

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

        private void RegisterHostedService<THostedService>()
            where THostedService : SuperSocketService<TReceivePackage>
        {
            base.HostBuilder.ConfigureServices((context, services) =>
            {
                RegisterHostedService<THostedService>(services);
            });
        }

        private void RegisterHostedService<THostedService>(IServiceCollection servicesInHost)
            where THostedService : SuperSocketService<TReceivePackage>
        {
            _currentServices.AddSingleton<THostedService>();
            servicesInHost.AddHostedService<THostedService>(s => GetHostedService<THostedService>());
        }

        protected override void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            RegisterHostedService<SuperSocketService<TReceivePackage>>(servicesInHost);
        }

        private THostedService GetHostedService<THostedService>()
        {
            var service = _serviceProvider.GetService<THostedService>();

            return service;
        }

        public override SuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
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