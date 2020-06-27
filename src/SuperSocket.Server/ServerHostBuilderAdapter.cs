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

        private IServiceCollection _currentServices;

        private List<Action<HostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();

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
            var services = _currentServices = new ServiceCollection();

            CopyGlobalServices(hostServices, services);

            services.AddOptions();
            services.AddLogging();      

            foreach (var configureServicesAction in _configureServicesActions)
            {
                configureServicesAction(context, services);
            }
        }

        public override IHost Build()
        {
            throw new NotSupportedException();
        }


        public override SuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureServicesActions.Add(configureDelegate);
            return this;
        }
    }
}