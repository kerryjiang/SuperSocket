using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.Server.Host
{
    /// <summary>
    /// Provides a builder for configuring and managing multiple SuperSocket servers.
    /// </summary>
    public class MultipleServerHostBuilder : HostBuilderAdapter<MultipleServerHostBuilder>, IMinimalApiHostBuilder
    {
        private List<IServerHostBuilderAdapter> _hostBuilderAdapters = new List<IServerHostBuilderAdapter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleServerHostBuilder"/> class with default settings.
        /// </summary>
        private MultipleServerHostBuilder()
            : this(args: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleServerHostBuilder"/> class with the specified arguments.
        /// </summary>
        /// <param name="args">The command-line arguments for the host builder.</param>
        private MultipleServerHostBuilder(string[] args)
            : base(args)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleServerHostBuilder"/> class with the specified host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder to adapt.</param>
        internal MultipleServerHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
        }

        /// <summary>
        /// Configures the servers with the specified host builder context and services.
        /// </summary>
        /// <param name="context">The context of the host builder.</param>
        /// <param name="hostServices">The collection of services for the host.</param>
        protected virtual void ConfigureServers(HostBuilderContext context, IServiceCollection hostServices)
        {
            foreach (var adapter in _hostBuilderAdapters)
            {
                adapter.ConfigureServer(context, hostServices);
            }
        }

        /// <summary>
        /// Builds the host and configures multiple servers.
        /// </summary>
        /// <returns>The built host.</returns>
        public override IHost Build()
        {
            this.ConfigureServices(ConfigureServers);

            var host = base.Build();
            var services = host.Services;

            AdaptMultipleServerHost(services);
            
            return host;
        }

        /// <summary>
        /// Adapts the services to support multiple servers.
        /// </summary>
        /// <param name="services">The service provider for the host.</param>
        internal void AdaptMultipleServerHost(IServiceProvider services)
        {
            foreach (var adapter in _hostBuilderAdapters)
            {
                adapter.ConfigureServiceProvider(services);
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MultipleServerHostBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="MultipleServerHostBuilder"/>.</returns>
        public static MultipleServerHostBuilder Create()
        {
            return Create(args: null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MultipleServerHostBuilder"/> class with the specified arguments.
        /// </summary>
        /// <param name="args">The command-line arguments for the host builder.</param>
        /// <returns>A new instance of <see cref="MultipleServerHostBuilder"/>.</returns>
        public static MultipleServerHostBuilder Create(string[] args)
        {
            return new MultipleServerHostBuilder(args);
        }

        private ServerHostBuilderAdapter<TReceivePackage> CreateServerHostBuilder<TReceivePackage>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate, string serverName = null)
            where TReceivePackage : class
        {
            var hostBuilder = new ServerHostBuilderAdapter<TReceivePackage>(this, serverName);            
            hostBuilderDelegate(hostBuilder);
            return hostBuilder;
        }

        /// <summary>
        /// Adds a server to the host builder with the specified configuration.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the server.</typeparam>
        /// <param name="hostBuilderDelegate">The action to configure the server host builder.</param>
        /// <param name="serverName">The optional server name for keyed service registration. Use this to register multiple instances of the same server type.</param>
        /// <returns>The updated host builder.</returns>
        public MultipleServerHostBuilder AddServer<TReceivePackage>(Action<ISuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate, string serverName = null)
            where TReceivePackage : class
        {
            var hostBuilder = CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate, serverName);
            _hostBuilderAdapters.Add(hostBuilder);
            return this;
        }

        /// <summary>
        /// Adds a server to the host builder with the specified configuration and pipeline filter.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the server.</typeparam>
        /// <typeparam name="TPipelineFilter">The type of the pipeline filter.</typeparam>
        /// <param name="hostBuilderDelegate">The action to configure the server host builder.</param>
        /// <param name="serverName">The optional server name for keyed service registration. Use this to register multiple instances of the same server type.</param>
        /// <returns>The updated host builder.</returns>
        public MultipleServerHostBuilder AddServer<TReceivePackage, TPipelineFilter>(Action<ISuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate, string serverName = null)
            where TReceivePackage : class
            where TPipelineFilter : class, IPipelineFilter<TReceivePackage>
        {            
            var hostBuilder = CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate, serverName);
            _hostBuilderAdapters.Add(hostBuilder);
            hostBuilder.UsePipelineFilter<TPipelineFilter>();
            return this;
        }

        /// <summary>
        /// Adds a server to the host builder using the specified server host builder adapter.
        /// </summary>
        /// <param name="hostBuilderAdapter">The server host builder adapter to add.</param>
        /// <returns>The updated host builder.</returns>
        public MultipleServerHostBuilder AddServer(IServerHostBuilderAdapter hostBuilderAdapter)
        {            
            _hostBuilderAdapters.Add(hostBuilderAdapter);
            return this;
        }

        /// <summary>
        /// Adds a server to the host builder with the specified service, package, and pipeline filter types.
        /// </summary>
        /// <typeparam name="TSuperSocketService">The type of the SuperSocket service.</typeparam>
        /// <typeparam name="TReceivePackage">The type of the package received by the server.</typeparam>
        /// <typeparam name="TPipelineFilter">The type of the pipeline filter.</typeparam>
        /// <param name="hostBuilderDelegate">The action to configure the server host builder.</param>
        /// <param name="serverName">The optional server name for keyed service registration. Use this to register multiple instances of the same server type.</param>
        /// <returns>The updated host builder.</returns>
        public MultipleServerHostBuilder AddServer<TSuperSocketService, TReceivePackage, TPipelineFilter>(Action<SuperSocketHostBuilder<TReceivePackage>> hostBuilderDelegate, string serverName = null)
            where TReceivePackage : class
            where TPipelineFilter : class, IPipelineFilter<TReceivePackage>
            where TSuperSocketService : SuperSocketService<TReceivePackage>
        {
            var hostBuilder = CreateServerHostBuilder<TReceivePackage>(hostBuilderDelegate, serverName);

            _hostBuilderAdapters.Add(hostBuilder);

            hostBuilder
                .UsePipelineFilter<TPipelineFilter>()
                .UseHostedService<TSuperSocketService>();
            return this;
        }

        /// <summary>
        /// Converts the host builder to a minimal API host builder.
        /// </summary>
        /// <returns>An instance of <see cref="IMinimalApiHostBuilder"/>.</returns>
        public IMinimalApiHostBuilder AsMinimalApiHostBuilder()
        {
            return this as IMinimalApiHostBuilder;
        }

        /// <summary>
        /// Configures the host builder for minimal API support.
        /// </summary>
        void IMinimalApiHostBuilder.ConfigureHostBuilder()
        {
            this.ConfigureServices(ConfigureServers);
            HostBuilder.ConfigureServices((_, services) => services.AddSingleton<MultipleServerHostBuilder>(this));
        }
    }
}