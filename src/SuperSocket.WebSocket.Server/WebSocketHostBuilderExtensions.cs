using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;

namespace SuperSocket.WebSocket.Server
{
    public static class WebSocketServerExtensions
    {
        public static WebSocketHostBuilder ConfigureWebSocketMessageHandler(this WebSocketHostBuilder builder, Func<WebSocketSession, WebSocketPackage, Task> handler)
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton<Func<WebSocketSession, WebSocketPackage, Task>>(handler);
            }) as WebSocketHostBuilder;
        }

        public static WebSocketHostBuilder UseCommand<TPackageInfo, TPackageMapper>(this WebSocketHostBuilder builder)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
            var keyType = CommandMiddlewareExtensions.GetKeyType<TPackageInfo>();
            var commandMiddlewareType = typeof(WebSocketCommandMiddleware<,,>).MakeGenericType(keyType, typeof(TPackageInfo), typeof(TPackageMapper));
            
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton(typeof(IWebSocketCommandMiddleware), commandMiddlewareType);
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
    }
}
