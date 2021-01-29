using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public static class CommandMiddlewareExtensions
    {
        public static Type GetKeyType<TPackageInfo>()
        {
            var interfaces = typeof(TPackageInfo).GetInterfaces();
            var keyInterface = interfaces.FirstOrDefault(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IKeyedPackageInfo<>));
            
            if (keyInterface == null)
                throw new Exception($"The package type {nameof(TPackageInfo)} should implement the interface {typeof(IKeyedPackageInfo<>).Name}.");

            return keyInterface.GetGenericArguments().FirstOrDefault();
        }

        private static ISuperSocketHostBuilder ConfigureCommand(this ISuperSocketHostBuilder builder)
        {
            return builder.ConfigureServices((hostCxt, services) =>
                {
                    services.Configure<CommandOptions>(hostCxt.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
                }) as ISuperSocketHostBuilder;
        }

        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TPackageInfo>(this ISuperSocketHostBuilder<TPackageInfo> builder)
            where TPackageInfo : class
        {
            var keyType = GetKeyType<TPackageInfo>();

            var useCommandMethod = typeof(CommandMiddlewareExtensions).GetMethod("UseCommand",  new Type[] { typeof(ISuperSocketHostBuilder) });
            useCommandMethod = useCommandMethod.MakeGenericMethod(keyType, typeof(TPackageInfo));

            var hostBuilder = useCommandMethod.Invoke(null, new object[] { builder }) as ISuperSocketHostBuilder;
            return hostBuilder.ConfigureCommand() as ISuperSocketHostBuilder<TPackageInfo>;
        }

        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TPackageInfo>(this ISuperSocketHostBuilder<TPackageInfo> builder, Action<CommandOptions> configurator)
            where TPackageInfo : class
        {
             return builder.UseCommand()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                }) as ISuperSocketHostBuilder<TPackageInfo>;
        }

        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TKey, TPackageInfo>(this ISuperSocketHostBuilder<TPackageInfo> builder, Action<CommandOptions> configurator, IEqualityComparer<TKey> comparer)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
            return builder.UseCommand(configurator)
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddSingleton<IEqualityComparer<TKey>>(comparer);
                }) as ISuperSocketHostBuilder<TPackageInfo>;
        }

        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TKey, TPackageInfo>(this ISuperSocketHostBuilder builder)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
            return builder.UseMiddleware<CommandMiddleware<TKey, TPackageInfo>>()
                .ConfigureCommand() as ISuperSocketHostBuilder<TPackageInfo>;
        }

        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TKey, TPackageInfo>(this ISuperSocketHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
             return builder.UseCommand<TKey, TPackageInfo>()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                }) as ISuperSocketHostBuilder<TPackageInfo>;
        }

        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TKey, TPackageInfo>(this ISuperSocketHostBuilder builder, Action<CommandOptions> configurator, IEqualityComparer<TKey> comparer)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
            return builder.UseCommand<TKey, TPackageInfo>(configurator)
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddSingleton<IEqualityComparer<TKey>>(comparer);
                }) as ISuperSocketHostBuilder<TPackageInfo>;
        }
    }
}
