using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.WebSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using System.Threading.Tasks;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    public interface IWebSocketHostBuilder : IHostBuilder<WebSocketPackage>
    {
        
    }

    class InternalWebSocketHostBuilder : SuperSocketHostBuilder<WebSocketPackage>, IWebSocketHostBuilder
    {
        public override IHost Build()
        {
            this.ConfigureServices((ctx, services) => 
            {
                services.TryAdd(new ServiceDescriptor(typeof(ISessionFactory), typeof(GenericSessionFactory<WebSocketSession>), ServiceLifetime.Singleton));
            });

            return base.Build();
        }
    }

    public static class WebSocketHostBuilder
    {
        public static IWebSocketHostBuilder Create()
        {
            return Create<WebSocketService>();
        }

        public static IWebSocketHostBuilder Create<TWebSocketService>()
            where TWebSocketService : WebSocketService
        {
            return new InternalWebSocketHostBuilder()
                .ConfigureDefaults()
                .UseSuperSocketWebSocket<TWebSocketService>()
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

        public static IWebSocketHostBuilder UseCommand<TPackageInfo, TPackageMapper>(this IWebSocketHostBuilder builder)
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
            }) as IWebSocketHostBuilder;
        }        

        public static IWebSocketHostBuilder UseCommand<TPackageInfo, TPackageMapper>(this IWebSocketHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
             return builder.UseCommand<TPackageInfo, TPackageMapper>()
                .ConfigureServices((ctx, services) =>
                {
                    services.Configure(configurator);
                }) as IWebSocketHostBuilder;
        }
    }
}
