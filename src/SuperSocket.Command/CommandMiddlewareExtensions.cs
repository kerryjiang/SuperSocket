using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;

namespace SuperSocket
{
    public static class CommandMiddlewareExtensions
    {
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
