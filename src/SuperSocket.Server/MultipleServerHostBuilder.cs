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
        private List<IServerHostBuilderAdapter> _hostBuilderAdapters = new List<IServerHostBuilderAdapter>();

        private MultipleServerHostBuilder()
            : this(args: null)
        {

        }

        private MultipleServerHostBuilder(string[] args)
            : base(args)
        {

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
            this.ConfigureServices(ConfigureServers);

            var host = base.Build();
            var services = host.Services;

            foreach (var adapter in _hostBuilderAdapters)
            {
                adapter.ConfigureServiceProvider(services);
            }
            
            return host;
        }

        public static MultipleServerHostBuilder Create()
        {
            return Create(args: null);
        }

        public static MultipleServerHostBuilder Create(string[] args)
        {
            return new MultipleServerHostBuilder(args);
        }

        private ServerHostBuilderAdapter<TReceivePackage> CreateServerHostBuilder<TReceivePackage>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
        {
            var hostBuilder = new ServerHostBuilderAdapter<TReceivePackage>(this);            
            hostBuilderDelegate(hostBuilder);
            _hostBuilderAdapters.Add(hostBuilder);
            return hostBuilder;
        }

        public MultipleServerHostBuilder AddServer<TReceivePackage>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
        {
            CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate);
            return this;
        }

        public MultipleServerHostBuilder AddServer<TReceivePackage, TPipelineFilter>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate)
                .UsePipelineFilter<TPipelineFilter>();
            return this;
        }

        public MultipleServerHostBuilder AddServer<TSuperSocketService, TReceivePackage, TPipelineFilter>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
            where TSuperSocketService : SuperSocketService<TReceivePackage>
        {
            CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate)
                .UsePipelineFilter<TPipelineFilter>()
                .UseHostedService<TSuperSocketService>();
            return this;
        }
    }
}