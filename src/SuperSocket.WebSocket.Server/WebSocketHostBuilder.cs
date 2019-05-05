using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.WebSocket;
using SuperSocket.Command;

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
                .UseSuperSocketWebSocket() as IWebSocketHostBuilder;
        }

        public static IWebSocketHostBuilder UseCommand<TKey, TPackageInfo, TPackageMapper>(this IWebSocketHostBuilder builder)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
            return builder.UseMiddleware<CommandMiddleware<TKey, WebSocketPackage, TPackageInfo, TPackageMapper>>()
                .ConfigureServices((hostCxt, services) =>
                {
                    services.Configure<CommandOptions>(hostCxt.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
                }) as IWebSocketHostBuilder;
        }

        public static IWebSocketHostBuilder UseCommand<TKey, TPackageInfo, TPackageMapper>(this IWebSocketHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
             return builder.UseCommand<TKey, TPackageInfo, TPackageMapper>()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                }) as IWebSocketHostBuilder;
        }
    }
}
