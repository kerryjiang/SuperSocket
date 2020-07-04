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
            : this(args: null)
        {

        }

        internal WebSocketHostBuilder(string[] args)
            : base(args)
        {

        }

        public override IHost Build()
        {
            this.ConfigureServices((ctx, services) =>
            {
                services.TryAddSingleton<ISessionFactory, GenericSessionFactory<WebSocketSession>>();
            })
            .UseSession<WebSocketSession>();
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
            return Create(args: null);
        }

        public static WebSocketHostBuilder Create(string[] args)
        {
            return ((new WebSocketHostBuilder(args) as SuperSocketHostBuilder<WebSocketPackage>)
                .UsePipelineFilter<WebSocketPipelineFilter>() as WebSocketHostBuilder)
                .UseMiddleware<HandshakeCheckMiddleware>()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
                }) as WebSocketHostBuilder;
        }
    }
}
