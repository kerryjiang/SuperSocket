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
    public class MultipleServerHostBuilder : HostBuilderAdapter<MultipleServerHostBuilder>
    {
        private bool _appConfigSet = false;

        private List<IServerHostBuilderAdapter> _hostBuilderAdapters = new List<IServerHostBuilderAdapter>();

        private MultipleServerHostBuilder()
        {

        }

        public override MultipleServerHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext,IConfigurationBuilder> configDelegate)
        {
            _appConfigSet = true;
            return this.ConfigureAppConfiguration(configDelegate);
        }

        protected virtual void ConfigureServers(HostBuilderContext context, IServiceCollection hostServices)
        {
            foreach (var adapter in _hostBuilderAdapters)
            {
                adapter.ConfigureServer(context, hostServices);
            }
        }

        public override IHost Build()
        {
            if (!_appConfigSet)
                this.ConfigureAppConfiguration(SuperSocketHostBuilder.ConfigureAppConfiguration);
                
            this.ConfigureServices(ConfigureServers);
            return base.Build();
        }

        public static MultipleServerHostBuilder Create()
        {
            return new MultipleServerHostBuilder();
        }

        public MultipleServerHostBuilder AddServer<TReceivePackage>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
        {
            var hostBuilder = new ServerHostBuilderAdapter<TReceivePackage>(this);            
            hostBuilderDelegate(hostBuilder);
            _hostBuilderAdapters.Add(hostBuilder);
            return this;
        }

        public MultipleServerHostBuilder AddServer<TReceivePackage, TPipelineFilter>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            var hostBuilder = new ServerHostBuilderAdapter<TReceivePackage>(this)
                .UsePipelineFilter<TPipelineFilter>();
            hostBuilderDelegate(hostBuilder);
            _hostBuilderAdapters.Add(hostBuilder as IServerHostBuilderAdapter);
            return this;
        }
    }
}