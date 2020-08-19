using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.Server;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Server
{
    class WebSocketHostBuilderAdapter : ServerHostBuilderAdapter<WebSocketPackage>
    {
        public WebSocketHostBuilderAdapter(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            this.UsePipelineFilter<WebSocketPipelineFilter>();
            this.UseMiddleware<HandshakeCheckMiddleware>();
            this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
            });
            this.Validator = WebSocketHostBuilder.ValidateHostBuilder;
        }

        protected override void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            services.TryAddSingleton<ISessionFactory, GenericSessionFactory<WebSocketSession>>();
        }

        protected override void RegisterDefaultHostedService(IServiceCollection servicesInHost)
        {
            servicesInHost.AddHostedService<WebSocketService>();
        }
    }

    public class WebSocketHostBuilder : SuperSocketHostBuilder<WebSocketPackage>
    {
        internal WebSocketHostBuilder()
            : this(args: null)
        {

        }

        internal WebSocketHostBuilder(IHostBuilder hostBuilder)
            : base(hostBuilder)
        {
            
        }

        internal WebSocketHostBuilder(string[] args)
            : base(args)
        {
            this.Validator = WebSocketHostBuilder.ValidateHostBuilder;
        }

        protected override void RegisterDefaultServices(HostBuilderContext builderContext, IServiceCollection servicesInHost, IServiceCollection services)
        {
            base.RegisterDefaultServices(builderContext, servicesInHost, services);
            services.TryAddSingleton<ISessionFactory, GenericSessionFactory<WebSocketSession>>();
        }

        protected override void RegisterDefaultHostedService(IServiceCollection services)
        {
            services.AddHostedService<WebSocketService>();
        }

        public new WebSocketHostBuilder UseHostedService<THostedService>()
            where THostedService : WebSocketService
        {
            return base.UseHostedService<THostedService>() as WebSocketHostBuilder;
        }

        public static WebSocketHostBuilder Create()
        {
            return Create(args: null);
        }

        public static WebSocketHostBuilder Create(string[] args)
        {
            return Create(new WebSocketHostBuilder(args));
        }

        public static WebSocketHostBuilder Create(SuperSocketHostBuilder<WebSocketPackage> hostBuilder)
        {
            return hostBuilder.UsePipelineFilter<WebSocketPipelineFilter>()
                .UseMiddleware<HandshakeCheckMiddleware>()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
                }) as WebSocketHostBuilder;
        }

        public static WebSocketHostBuilder Create(IHostBuilder hostBuilder)
        {
            return Create(new WebSocketHostBuilder(hostBuilder));
        }

        internal static void ValidateHostBuilder(HostBuilderContext builderCtx, IServiceCollection services)
        {
            
        }
    }
}
