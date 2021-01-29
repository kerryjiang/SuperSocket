using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SuperSocket.Command;
using SuperSocket.Server;
using SuperSocket.WebSocket.Server.Extensions;
using SuperSocket.WebSocket.Server.Extensions.Compression;

namespace SuperSocket.WebSocket.Server
{
    public static class WebSocketServerExtensions
    {
        internal static ISuperSocketHostBuilder<WebSocketPackage> UseWebSocketMiddleware(this ISuperSocketHostBuilder<WebSocketPackage> builder)
        {
            return builder
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IWebSocketServerMiddleware, WebSocketServerMiddleware>();
                })
                .UseMiddleware<WebSocketServerMiddleware>(s => s.GetService<IWebSocketServerMiddleware>() as WebSocketServerMiddleware)
                as ISuperSocketHostBuilder<WebSocketPackage>;
        }

        public static ISuperSocketHostBuilder<WebSocketPackage> UseWebSocketMessageHandler(this ISuperSocketHostBuilder<WebSocketPackage> builder, Func<WebSocketSession, WebSocketPackage, ValueTask> handler)
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.AddSingleton<Func<WebSocketSession, WebSocketPackage, ValueTask>>(handler);
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }

        public static ISuperSocketHostBuilder<WebSocketPackage> UseWebSocketMessageHandler(this ISuperSocketHostBuilder<WebSocketPackage> builder, string protocol, Func<WebSocketSession, WebSocketPackage, ValueTask> handler)
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ISubProtocolHandler), new DelegateSubProtocolHandler(protocol, handler)));
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }

        public static ISuperSocketHostBuilder<WebSocketPackage> UseCommand<TPackageInfo, TPackageMapper>(this ISuperSocketHostBuilder<WebSocketPackage> builder)
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
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        } 

        public static ISuperSocketHostBuilder<WebSocketPackage> UseCommand<TPackageInfo, TPackageMapper>(this ISuperSocketHostBuilder<WebSocketPackage> builder, Action<CommandOptions> configurator)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>, new()
        {
             return builder.UseCommand<TPackageInfo, TPackageMapper>()
                .ConfigureServices((ctx, services) =>
                {
                    services.Configure(configurator);
                }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }

        public static ISuperSocketHostBuilder<WebSocketPackage> UseCommand<TPackageInfo, TPackageMapper>(this ISuperSocketHostBuilder<WebSocketPackage> builder, string protocol, Action<CommandOptions> commandOptionsAction = null)
            where TPackageInfo : class
            where TPackageMapper : class, IPackageMapper<WebSocketPackage, TPackageInfo>
        {
            
            return builder.ConfigureServices((ctx, services) => 
            {                
                var commandOptions = new CommandOptions();                
                ctx.Configuration?.GetSection("serverOptions")?.GetSection("commands")?.GetSection(protocol)?.Bind(commandOptions);                
                commandOptionsAction?.Invoke(commandOptions);
                var commandOptionsWrapper = new OptionsWrapper<CommandOptions>(commandOptions);

                services.TryAddEnumerable(ServiceDescriptor.Singleton<ISubProtocolHandler, CommandSubProtocolHandler<TPackageInfo>>((sp) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance<TPackageMapper>(sp);
                    return new CommandSubProtocolHandler<TPackageInfo>(protocol, sp, commandOptionsWrapper, mapper);
                }));
            }) as ISuperSocketHostBuilder<WebSocketPackage>;
        }

        public static ISuperSocketHostBuilder<WebSocketPackage> UsePerMessageCompression(this ISuperSocketHostBuilder<WebSocketPackage> builder)
        {
             return builder.ConfigureServices((ctx, services) =>
             {
                 services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebSocketExtensionFactory, WebSocketPerMessageCompressionExtensionFactory>());
             });
        }

        public static MultipleServerHostBuilder AddWebSocketServer(this MultipleServerHostBuilder hostBuilder, Action<ISuperSocketHostBuilder<WebSocketPackage>> hostBuilderDelegate)
        {
            return hostBuilder.AddWebSocketServer<SuperSocketService<WebSocketPackage>>(hostBuilderDelegate);
        }

        public static MultipleServerHostBuilder AddWebSocketServer<TWebSocketService>(this MultipleServerHostBuilder hostBuilder, Action<ISuperSocketHostBuilder<WebSocketPackage>> hostBuilderDelegate)
            where TWebSocketService : SuperSocketService<WebSocketPackage>
        {
            var appHostBuilder = new WebSocketHostBuilderAdapter(hostBuilder);

            appHostBuilder
                .UseHostedService<TWebSocketService>();

            hostBuilderDelegate?.Invoke(appHostBuilder);

            hostBuilder.AddServer(appHostBuilder);
            return hostBuilder;
        }

        public static WebSocketHostBuilder AsWebSocketHostBuilder(this IHostBuilder hostBuilder)
        {
            return WebSocketHostBuilder.Create(hostBuilder);
        }
    }
}
