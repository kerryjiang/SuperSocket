using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.WebSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    public interface IWebSocketHostBuilder : IHostBuilder<WebSocketPackage>
    {
        
    }

    class InternalWebSocketHostBuilder : SuperSocketHostBuilder<WebSocketPackage>, IWebSocketHostBuilder
    {

    }

    public static class WebSocketHostBuilder
    {
        public static IWebSocketHostBuilder Create()
        {
            return new InternalWebSocketHostBuilder()
                .ConfigureDefaults()
                .UseSuperSocketWebSocket()
                .UseMiddleware<HandshakeCheckMiddleware>()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IPackageHandler<WebSocketPackage>, WebSocketPackageHandler>();
                }) as IWebSocketHostBuilder;
        }

        public static IWebSocketHostBuilder ConfigureWebSocketMessageHandler(this IWebSocketHostBuilder builder, Func<WebSocketSession, WebSocketPackage, Task> handler)
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton<Func<WebSocketSession, WebSocketPackage, Task>>(handler);
            }) as IWebSocketHostBuilder;
        }

        public static IWebSocketHostBuilder UseCommand<TKey, TPackageInfo, TPackageMapper>(this IWebSocketHostBuilder builder)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton<IWebSocketCommandMiddleware, WebSocketCommandMiddleware<TKey, TPackageInfo, TPackageMapper>>();
            }).ConfigureServices((ctx, services) =>
            {
                services.Configure<CommandOptions>(ctx.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
            }) as IWebSocketHostBuilder;
        }        

        public static IWebSocketHostBuilder UseCommand<TKey, TPackageInfo>(this IWebSocketHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
             return builder.UseCommand<TKey, TPackageInfo>()
                .ConfigureServices((ctx, services) =>
                {
                    services.Configure(configurator);
                }) as IWebSocketHostBuilder;
        }
    }
}
