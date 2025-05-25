using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Connection;
using System.IO.Compression;

namespace SuperSocket.Server.Host
{
    /// <summary>
    /// Provides extension methods for configuring and building SuperSocket hosts.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Converts an <see cref="IHostBuilder"/> to an <see cref="ISuperSocketHostBuilder{TReceivePackage}"/>.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to convert.</param>
        /// <returns>An instance of <see cref="ISuperSocketHostBuilder{TReceivePackage}"/>.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> AsSuperSocketHostBuilder<TReceivePackage>(this IHostBuilder hostBuilder)
        {
            if (hostBuilder is ISuperSocketHostBuilder<TReceivePackage> ssHostBuilder)
            {
                return ssHostBuilder;
            }

            return new SuperSocketHostBuilder<TReceivePackage>(hostBuilder);
        }

        /// <summary>
        /// Converts an <see cref="IHostBuilder"/> to an <see cref="ISuperSocketHostBuilder{TReceivePackage}"/> with a specified pipeline filter.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <typeparam name="TPipelineFilter">The type of the pipeline filter.</typeparam>
        /// <param name="hostBuilder">The host builder to convert.</param>
        /// <returns>An instance of <see cref="ISuperSocketHostBuilder{TReceivePackage}"/>.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> AsSuperSocketHostBuilder<TReceivePackage, TPipelineFilter>(this IHostBuilder hostBuilder)
            where TPipelineFilter : class, IPipelineFilter<TReceivePackage>
        {
            if (hostBuilder is ISuperSocketHostBuilder<TReceivePackage> ssHostBuilder)
            {
                return ssHostBuilder;
            }

            return (new SuperSocketHostBuilder<TReceivePackage>(hostBuilder))
                .UsePipelineFilter<TPipelineFilter>();
        }

        /// <summary>
        /// Configures a pipeline filter factory for the host builder.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <param name="filterFactory">The factory function to create pipeline filters.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IPipelineFilter<TReceivePackage>> filterFactory)
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IPipelineFilter<TReceivePackage>>>(filterFactory);
                }
            );

            return hostBuilder.UsePipelineFilterFactory<DelegatePipelineFilterFactory<TReceivePackage>>();
        }

        /// <summary>
        /// Configures the host builder to use middleware for clearing idle sessions.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UseClearIdleSession<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return hostBuilder.UseMiddleware<ClearIdleSessionMiddleware>();
        }

        /// <summary>
        /// Configures session handlers for the host builder.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <param name="onConnected">The handler for session connected events.</param>
        /// <param name="onClosed">The handler for session closed events.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UseSessionHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, ValueTask> onConnected = null, Func<IAppSession, CloseEventArgs, ValueTask> onClosed = null)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<SessionHandlers>(new SessionHandlers
                    {
                        Connected = onConnected,
                        Closed = onClosed
                    });
                }
            );
        }

        /// <summary>
        /// Configures SuperSocket server options for the host builder.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <param name="configurator">The action to configure server options.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureSuperSocket<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<ServerOptions> configurator)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            );
        }

        /// <summary>
        /// Configures socket options for the host builder.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <param name="socketOptionsSetter">The action to configure socket options.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureSocketOptions<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<Socket> socketOptionsSetter)
            where TReceivePackage : class
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<SocketOptionsSetter>(new SocketOptionsSetter(socketOptionsSetter));
                }
            );
        }

        /// <summary>
        /// Builds the host as a server.
        /// </summary>
        /// <param name="hostBuilder">The host builder to build.</param>
        /// <returns>The built server.</returns>
        public static IServer BuildAsServer(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return host.AsServer();
        }

        /// <summary>
        /// Converts an <see cref="IHost"/> to an <see cref="IServer"/>.
        /// </summary>
        /// <param name="host">The host to convert.</param>
        /// <returns>The server instance.</returns>
        public static IServer AsServer(this IHost host)
        {
            return host.Services.GetService<IEnumerable<IHostedService>>().OfType<IServer>().FirstOrDefault();
        }

        /// <summary>
        /// Configures an error handler for the host builder.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <param name="errorHandler">The error handler function.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureErrorHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }

        /// <summary>
        /// Configures a package handler for the host builder.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package received by the host.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <param name="packageHandler">The package handler function.</param>
        /// <param name="errorHandler">The error handler function.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UsePackageHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, ValueTask> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    if (packageHandler != null)
                        services.AddSingleton<IPackageHandler<TReceivePackage>>(new DelegatePackageHandler<TReceivePackage>(packageHandler));

                    if (errorHandler != null)
                        services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }

        /// <summary>
        /// Converts an <see cref="IHostBuilder"/> to a <see cref="MultipleServerHostBuilder"/>.
        /// </summary>
        /// <param name="hostBuilder">The host builder to convert.</param>
        /// <returns>An instance of <see cref="MultipleServerHostBuilder"/>.</returns>
        public static MultipleServerHostBuilder AsMultipleServerHostBuilder(this IHostBuilder hostBuilder)
        {
            return new MultipleServerHostBuilder(hostBuilder);
        }

        /// <summary>
        /// Converts an <see cref="IHostApplicationBuilder"/> to a <see cref="SuperSocketApplicationBuilder"/>.
        /// </summary>
        /// <param name="hostApplicationBuilder">The host application builder to convert.</param>
        /// <param name="configureServerHostBuilder">The action to configure the server host builder.</param>
        /// <returns>An instance of <see cref="SuperSocketApplicationBuilder"/>.</returns>
        [Obsolete("Use AsSuperSocketApplicationBuilder instead.")]
        public static SuperSocketApplicationBuilder AsSuperSocketWebApplicationBuilder(this IHostApplicationBuilder hostApplicationBuilder, Action<MultipleServerHostBuilder> configureServerHostBuilder)
        {
            return hostApplicationBuilder.AsSuperSocketApplicationBuilder(configureServerHostBuilder);
        }

        /// <summary>
        /// Converts an <see cref="IHostApplicationBuilder"/> to a <see cref="SuperSocketApplicationBuilder"/>.
        /// </summary>
        /// <param name="hostApplicationBuilder">The host application builder to convert.</param>
        /// <param name="configureServerHostBuilder">The action to configure the server host builder.</param>
        /// <returns>An instance of <see cref="SuperSocketApplicationBuilder"/>.</returns>
        public static SuperSocketApplicationBuilder AsSuperSocketApplicationBuilder(this IHostApplicationBuilder hostApplicationBuilder, Action<MultipleServerHostBuilder> configureServerHostBuilder)
        {
            var applicationBuilder = new SuperSocketApplicationBuilder(hostApplicationBuilder);

            var hostBuilder = new MultipleServerHostBuilder(applicationBuilder.Host);
            configureServerHostBuilder(hostBuilder);
            hostBuilder.AsMinimalApiHostBuilder().ConfigureHostBuilder();
            return applicationBuilder;
        }

        /// <summary>
        /// Converts an <see cref="ISuperSocketHostBuilder"/> to a <see cref="IMinimalApiHostBuilder"/>.
        /// </summary>
        /// <param name="hostBuilder">The host builder to convert.</param>
        /// <returns>An instance of <see cref="IMinimalApiHostBuilder"/>.</returns>
        public static IMinimalApiHostBuilder AsMinimalApiHostBuilder(this ISuperSocketHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        /// <summary>
        /// Configures the host builder to use GZip compression.
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder UseGZip(this ISuperSocketHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((hostCtx, services) =>
            {
                services.AddSingleton<IConnectionStreamInitializersFactory>(new DefaultConnectionStreamInitializersFactory(CompressionLevel.Optimal));
            }) as ISuperSocketHostBuilder;
        }
    }
}
