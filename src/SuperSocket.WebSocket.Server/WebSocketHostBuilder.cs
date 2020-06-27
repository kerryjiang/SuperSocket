using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.Server;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketHostBuilder : SuperSocketHostBuilder<WebSocketPackage>
    {
        internal WebSocketHostBuilder()
        {

        }
        public override IHost Build()
        {
            this.ConfigureServices((ctx, services) => 
            {
                services.TryAdd(new ServiceDescriptor(typeof(ISessionFactory), typeof(GenericSessionFactory<WebSocketSession>), ServiceLifetime.Singleton));
            });

            return base.Build();
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

        public new SuperSocketHostBuilder<WebSocketPackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<WebSocketPackage>, new()
        {
            throw new NotSupportedException();
        }

        public new SuperSocketHostBuilder<WebSocketPackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<WebSocketPackage>
        {
            throw new NotSupportedException();
        }

        public static WebSocketHostBuilder Create()
        {
            return ((new WebSocketHostBuilder() as SuperSocketHostBuilder<WebSocketPackage>)
                .UsePipelineFilter<WebSocketPipelineFilter>() as WebSocketHostBuilder)
                .UseMiddleware<HandshakeCheckMiddleware>()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
                }) as WebSocketHostBuilder;
        }
    }
}
