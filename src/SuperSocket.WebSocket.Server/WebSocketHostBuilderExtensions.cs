using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    public static class WebSocketServerExtensions
    {
        public static WebSocketHostBuilder UseWebSocketMessageHandler(this WebSocketHostBuilder builder, Func<WebSocketSession, WebSocketPackage, Task> handler)
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton<Func<WebSocketSession, WebSocketPackage, Task>>(handler);
            }) as WebSocketHostBuilder;
        }

        public static WebSocketHostBuilder UseCommand<TPackageInfo, TPackageMapper>(this WebSocketHostBuilder builder)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>
        {
            var keyType = CommandMiddlewareExtensions.GetKeyType<TPackageInfo>();
            var commandMiddlewareType = typeof(WebSocketCommandMiddleware<,>).MakeGenericType(keyType, typeof(TPackageInfo));
            
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton(typeof(IWebSocketCommandMiddleware), commandMiddlewareType);
                services.AddSingleton<IPackageMapper<WebSocketPackage, TPackageInfo>, TPackageMapper>();
            }).ConfigureServices((ctx, services) =>
            {
                services.Configure<CommandOptions>(ctx.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
            }) as WebSocketHostBuilder;
        }        

        public static WebSocketHostBuilder UseCommand<TPackageInfo, TPackageMapper>(this WebSocketHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
             return builder.UseCommand<TPackageInfo, TPackageMapper>()
                .ConfigureServices((ctx, services) =>
                {
                    services.Configure(configurator);
                }) as WebSocketHostBuilder;
        }

        public static MultipleServerHostBuilder AddWebSocketServer(this MultipleServerHostBuilder hostBuilder, Action<SuperSocketHostBuilder<WebSocketPackage>> hostBuilderDelegate)
        {
            return hostBuilder.AddWebSocketServer<WebSocketService>(hostBuilderDelegate);
        }

        public static MultipleServerHostBuilder AddWebSocketServer<TWebSocketService>(this MultipleServerHostBuilder hostBuilder, Action<SuperSocketHostBuilder<WebSocketPackage>> hostBuilderDelegate)
            where TWebSocketService : WebSocketService
        {
            hostBuilder.AddServer<WebSocketPackage, WebSocketPipelineFilter>((hb) =>
            {
                hb
                .UseHostedService<TWebSocketService>()
                .UseMiddleware<HandshakeCheckMiddleware>()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
                });

                hostBuilderDelegate?.Invoke(hb);
            });
            return hostBuilder;
        }
    }
}
