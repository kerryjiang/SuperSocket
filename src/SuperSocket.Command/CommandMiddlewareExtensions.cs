using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket.Command
{
    public static class CommandMiddlewareExtensions
    {
        public static IHostBuilder UseCommand<TKey, TPackageInfo>(this IHostBuilder builder)
            where TPackageInfo : IKeyedPackageInfo<TKey>
        {
            return builder.UseMiddleware<CommandMiddleware<TKey, TPackageInfo>>()
                .ConfigureServices((hostCxt, services) =>
                {
                    services.Configure<CommandOptions>(hostCxt.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
                });
        }

        public static IHostBuilder UseCommand<TKey, TPackageInfo>(this IHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : IKeyedPackageInfo<TKey>
        {
             return builder.UseCommand<TKey, TPackageInfo>()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                });
        }
    }
}
