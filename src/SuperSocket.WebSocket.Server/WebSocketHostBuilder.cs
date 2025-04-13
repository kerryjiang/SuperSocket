using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.Server;
using SuperSocket.Server.Connection;
using SuperSocket.Server.Host;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.WebSocket.Server
{
    class WebSocketHostBuilderAdapter : ServerHostBuilderAdapter<WebSocketPackage>
    {
        public WebSocketHostBuilderAdapter(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            this.UsePipelineFilter<WebSocketPipelineFilter>();
            this.UseWebSocketMiddleware();
            this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
            });
            this.ConfigureSupplementServices(WebSocketHostBuilder.ValidateHostBuilder);
        }

        protected override void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            services.TryAddSingleton<ISessionFactory, GenericSessionFactory<WebSocketSession>>();
            services.TryAddSingleton<IConnectionListenerFactory, TcpConnectionListenerFactory>();
            services.TryAddSingleton<SocketOptionsSetter>(new SocketOptionsSetter(socket => { }));
            services.TryAddSingleton<IConnectionFactoryBuilder, ConnectionFactoryBuilder>();
            services.TryAddSingleton<IConnectionStreamInitializersFactory, DefaultConnectionStreamInitializersFactory>();
        }
    }

    /// <summary>
    /// Represents a builder for configuring and creating WebSocket hosts.
    /// </summary>
    public class WebSocketHostBuilder : SuperSocketHostBuilder<WebSocketPackage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketHostBuilder"/> class.
        /// </summary>
        internal WebSocketHostBuilder()
            : this(args: null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketHostBuilder"/> class with the specified host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder to use.</param>
        internal WebSocketHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketHostBuilder"/> class with the specified arguments.
        /// </summary>
        /// <param name="args">The arguments to use.</param>
        internal WebSocketHostBuilder(string[] args)
            : base(args)
        {
            this.ConfigureSupplementServices(WebSocketHostBuilder.ValidateHostBuilder);
        }
        
        protected override void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            services.TryAddSingleton<ISessionFactory, GenericSessionFactory<WebSocketSession>>();
            base.RegisterDefaultServices(builderContext, servicesInHost, services);
        }        

        /// <summary>
        /// Creates a new instance of the <see cref="WebSocketHostBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="WebSocketHostBuilder"/> class.</returns>
        public static WebSocketHostBuilder Create()
        {
            return Create(args: null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WebSocketHostBuilder"/> class with the specified arguments.
        /// </summary>
        /// <param name="args">The arguments to use.</param>
        /// <returns>A new instance of the <see cref="WebSocketHostBuilder"/> class.</returns>
        public static WebSocketHostBuilder Create(string[] args)
        {
            return Create(new WebSocketHostBuilder(args));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WebSocketHostBuilder"/> class with the specified host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder to use.</param>
        /// <returns>A new instance of the <see cref="WebSocketHostBuilder"/> class.</returns>
        public static WebSocketHostBuilder Create(IHostBuilder hostBuilder)
        {
            return Create(new WebSocketHostBuilder(hostBuilder));
        }

        public static WebSocketHostBuilder Create(SuperSocketHostBuilder<WebSocketPackage> hostBuilder)
        {
            return hostBuilder.UsePipelineFilter<WebSocketPipelineFilter>()
                .UseWebSocketMiddleware()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
                }) as WebSocketHostBuilder;
        }

        internal static void ValidateHostBuilder(HostBuilderContext builderCtx, IServiceCollection services)
        {
            
        }
    }
}
