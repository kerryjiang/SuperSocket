using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;
using System.Reflection;

namespace SuperSocket
{
    public static class CommandMiddlewareExtensions
    {

        public static IHostBuilder<TPackageInfo> UseCommand<TPackageInfo>(this IHostBuilder<TPackageInfo> builder)
            where TPackageInfo : class
        {
            var interfaces = typeof(TPackageInfo).GetInterfaces();
            var keyInterface = interfaces.FirstOrDefault(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IKeyedPackageInfo<>));
            
            if (keyInterface == null)
                throw new Exception($"The package type {nameof(TPackageInfo)} should implement the interface {typeof(IKeyedPackageInfo<>).Name}.");

            var keyType = keyInterface.GetGenericArguments().FirstOrDefault();

            var useCommandMethod = typeof(CommandMiddlewareExtensions).GetMethod("UseCommand",  new Type[] { typeof(IHostBuilder) });
            useCommandMethod = useCommandMethod.MakeGenericMethod(keyType, typeof(TPackageInfo));

            return useCommandMethod.Invoke(null, new object[] { builder }) as IHostBuilder<TPackageInfo>;
        }

        public static IHostBuilder<TPackageInfo> UseCommand<TPackageInfo>(this IHostBuilder<TPackageInfo> builder, Action<CommandOptions> configurator)
            where TPackageInfo : class
        {
             return builder.UseCommand()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                }) as IHostBuilder<TPackageInfo>;
        }

        public static IHostBuilder UseCommand<TKey, TPackageInfo>(this IHostBuilder builder)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
            return builder.UseMiddleware<CommandMiddleware<TKey, TPackageInfo>>()
                .ConfigureServices((hostCxt, services) =>
                {
                    services.Configure<CommandOptions>(hostCxt.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
                });
        }

        public static IHostBuilder UseCommand<TKey, TPackageInfo>(this IHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
             return builder.UseCommand<TKey, TPackageInfo>()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                });
        }

         public static IHostBuilder UseCommand<TKey, TPackageInfo>(this IHostBuilder builder, Action<CommandOptions> configurator, IEqualityComparer<TKey> comparer)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
            return builder.UseCommand<TKey, TPackageInfo>(configurator)
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddSingleton<IEqualityComparer<TKey>>(comparer);
                });
        }
    }
}
