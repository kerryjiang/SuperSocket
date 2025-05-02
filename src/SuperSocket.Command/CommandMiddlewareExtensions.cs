using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.Server
{
    /// <summary>
    /// Provides extension methods for configuring and using command middleware in a SuperSocket application.
    /// </summary>
    public static class CommandMiddlewareExtensions
    {
        /// <summary>
        /// Gets the key type from the specified package type.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <returns>The key type of the package.</returns>
        /// <exception cref="Exception">Thrown if the package type does not implement <see cref="IKeyedPackageInfo{TKey}"/>.</exception>
        public static Type GetKeyType<TPackageInfo>()
        {
            var interfaces = typeof(TPackageInfo).GetInterfaces();
            var keyInterface = interfaces.FirstOrDefault(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IKeyedPackageInfo<>));
            
            if (keyInterface == null)
                throw new Exception($"The package type {nameof(TPackageInfo)} should implement the interface {typeof(IKeyedPackageInfo<>).Name}.");

            return keyInterface.GetGenericArguments().FirstOrDefault();
        }

        /// <summary>
        /// Configures command options for the SuperSocket host builder.
        /// </summary>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <returns>The configured host builder.</returns>
        private static ISuperSocketHostBuilder ConfigureCommand(this ISuperSocketHostBuilder builder)
        {
            return builder.ConfigureServices((hostCxt, services) =>
                {
                    services.Configure<CommandOptions>(hostCxt.Configuration?.GetSection("serverOptions")?.GetSection("commands"));
                }) as ISuperSocketHostBuilder;
        }

        /// <summary>
        /// Adds command middleware to the SuperSocket host builder.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TPackageInfo>(this ISuperSocketHostBuilder<TPackageInfo> builder)
            where TPackageInfo : class
        {
            var keyType = GetKeyType<TPackageInfo>();

            var useCommandMethod = typeof(CommandMiddlewareExtensions).GetMethod("UseCommand",  new Type[] { typeof(ISuperSocketHostBuilder) });
            useCommandMethod = useCommandMethod.MakeGenericMethod(keyType, typeof(TPackageInfo));

            var hostBuilder = useCommandMethod.Invoke(null, new object[] { builder }) as ISuperSocketHostBuilder;
            return hostBuilder.ConfigureCommand() as ISuperSocketHostBuilder<TPackageInfo>;
        }

        /// <summary>
        /// Adds command middleware to the SuperSocket host builder with a configurator.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <param name="configurator">The configurator for command options.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TPackageInfo>(this ISuperSocketHostBuilder<TPackageInfo> builder, Action<CommandOptions> configurator)
            where TPackageInfo : class
        {
             return builder.UseCommand()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                }) as ISuperSocketHostBuilder<TPackageInfo>;
        }

        /// <summary>
        /// Adds command middleware to the SuperSocket host builder with a configurator and a key comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the command key.</typeparam>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <param name="configurator">The configurator for command options.</param>
        /// <param name="comparer">The comparer for command keys.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TKey, TPackageInfo>(this ISuperSocketHostBuilder<TPackageInfo> builder, Action<CommandOptions> configurator, IEqualityComparer<TKey> comparer)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
            return builder.UseCommand(configurator)
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddSingleton<IEqualityComparer<TKey>>(comparer);
                }) as ISuperSocketHostBuilder<TPackageInfo>;
        }

        /// <summary>
        /// Adds command middleware to the SuperSocket host builder with a specific key type.
        /// </summary>
        /// <typeparam name="TKey">The type of the command key.</typeparam>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TKey, TPackageInfo>(this ISuperSocketHostBuilder builder)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
            return builder.UseMiddleware<CommandMiddleware<TKey, TPackageInfo>>()
                .ConfigureCommand() as ISuperSocketHostBuilder<TPackageInfo>;
        }

        /// <summary>
        /// Adds command middleware to the SuperSocket host builder with a specific key type and a configurator.
        /// </summary>
        /// <typeparam name="TKey">The type of the command key.</typeparam>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <param name="configurator">The configurator for command options.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TPackageInfo> UseCommand<TKey, TPackageInfo>(this ISuperSocketHostBuilder builder, Action<CommandOptions> configurator)
            where TPackageInfo : class, IKeyedPackageInfo<TKey>
        {
             return builder.UseCommand<TKey, TPackageInfo>()
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure(configurator);
                }) as ISuperSocketHostBuilder<TPackageInfo>;
        }

        /// <summary>
        /// Adds command middleware to the SuperSocket host builder with a specific key type, a configurator, and a key comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the command key.</typeparam>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <param name="configurator">The configurator for command options.</param>
        /// <param name="comparer">The comparer for command keys.</param>
        /// <returns>The configured host builder.</returns>
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
