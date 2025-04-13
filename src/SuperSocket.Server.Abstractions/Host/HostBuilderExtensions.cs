using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Middleware;

namespace SuperSocket.Server
{
    /// <summary>
    /// Provides extension methods for configuring a SuperSocket host builder.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Converts an <see cref="IHostBuilder"/> to an <see cref="ISuperSocketHostBuilder"/>.
        /// </summary>
        /// <param name="hostBuilder">The host builder to convert.</param>
        /// <returns>The converted SuperSocket host builder.</returns>
        public static ISuperSocketHostBuilder AsSuperSocketBuilder(this IHostBuilder hostBuilder)
        {
            return hostBuilder as ISuperSocketHostBuilder;
        }

        /// <summary>
        /// Adds a middleware to the SuperSocket host builder.
        /// </summary>
        /// <typeparam name="TMiddleware">The type of the middleware.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <returns>The updated SuperSocket host builder.</returns>
        public static ISuperSocketHostBuilder UseMiddleware<TMiddleware>(this ISuperSocketHostBuilder builder)
            where TMiddleware : class, IMiddleware
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
            }).AsSuperSocketBuilder();
        }

        /// <summary>
        /// Adds a middleware to the SuperSocket host builder with a custom implementation factory.
        /// </summary>
        /// <typeparam name="TMiddleware">The type of the middleware.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <param name="implementationFactory">The factory to create the middleware instance.</param>
        /// <returns>The updated SuperSocket host builder.</returns>
        public static ISuperSocketHostBuilder UseMiddleware<TMiddleware>(this ISuperSocketHostBuilder builder, Func<IServiceProvider, TMiddleware> implementationFactory)
            where TMiddleware : class, IMiddleware
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>(implementationFactory));
            }).AsSuperSocketBuilder();
        }

        /// <summary>
        /// Adds a TCP connection listener factory to the SuperSocket host builder.
        /// </summary>
        /// <typeparam name="TConnectionListenerFactory">The type of the connection listener factory.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <returns>The updated SuperSocket host builder.</returns>
        public static ISuperSocketHostBuilder UseTcpConnectionListenerFactory<TConnectionListenerFactory>(this ISuperSocketHostBuilder builder)
            where TConnectionListenerFactory : class, IConnectionListenerFactory
        {
            return builder.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IConnectionListenerFactory, TConnectionListenerFactory>();
            }).AsSuperSocketBuilder();
        }
    }
}
