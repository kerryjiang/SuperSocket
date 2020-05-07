using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Server;


namespace SuperSocket
{
    public static class HostBuilderExtensions
    {

        public static SuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
        {
            var builder = hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<object, IPipelineFilter<TReceivePackage>>>(filterFactory);
                }
            ) as SuperSocketHostBuilder<TReceivePackage>;

            return builder.UsePipelineFilterFactory<DelegatePipelineFilterFactory<TReceivePackage>>();
        }

  
        public static IHostBuilder UseSession<TSession>(this IHostBuilder hostBuilder)
            where TSession : AppSession, new()
        {
            return hostBuilder.UseSessionFactory<GenericSessionFactory<TSession>>();
        }

        public static IHostBuilder UseSessionFactory<TSessionFactory>(this IHostBuilder hostBuilder)
            where TSessionFactory : class, ISessionFactory
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<ISessionFactory, TSessionFactory>();
                }
            );

            return hostBuilder;
        }

        public static IHostBuilder UseClearIdleSession(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseMiddleware<ClearIdleSessionMiddleware>();
        }

        public static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageHandler<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, Task> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
            where TReceivePackage : class
        {
            return ConfigurePackageHandlerCore<TReceivePackage>(hostBuilder, packageHandler, errorHandler: errorHandler) as SuperSocketHostBuilder<TReceivePackage>;
        }

        private static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageHandlerCore<TReceivePackage>(SuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, Task> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
            where TReceivePackage : class
        {
            if (packageHandler == null)
            {
                return hostBuilder;
            }

            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageHandler<TReceivePackage>>(new DelegatePackageHandler<TReceivePackage>(packageHandler));

                    if (errorHandler != null)
                        services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            ) as SuperSocketHostBuilder<TReceivePackage>;
        }

        public static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageHandler<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, Task> packageHandler)
            where TReceivePackage : class
        {
            return ConfigurePackageHandlerCore<TReceivePackage>(hostBuilder, packageHandler) as SuperSocketHostBuilder<TReceivePackage>;
        }

        private static SuperSocketHostBuilder<TReceivePackage> ConfigureErrorHandler<TReceivePackage>(SuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler)
            where TReceivePackage : class
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            ) as SuperSocketHostBuilder<TReceivePackage>;
        }

        public static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageDecoder<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, IPackageDecoder<TReceivePackage> packageDecoder)
            where TReceivePackage : class
        {
            return ConfigurePackageDecoderCore<TReceivePackage>(hostBuilder, packageDecoder);
        }

        private static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageDecoderCore<TReceivePackage>(SuperSocketHostBuilder<TReceivePackage> hostBuilder, IPackageDecoder<TReceivePackage> packageDecoder)
            where TReceivePackage : class
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageDecoder<TReceivePackage>>(packageDecoder);
                }
            ) as SuperSocketHostBuilder<TReceivePackage>;
        }

        public static IHostBuilder ConfigureSessionHandler(this IHostBuilder hostBuilder, Func<IAppSession, ValueTask> onConnected = null, Func<IAppSession, ValueTask> onClosed = null)
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

        public static IHostBuilder ConfigureSuperSocket(this IHostBuilder hostBuilder, Action<ServerOptions> configurator)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            );
        }

        public static SuperSocketHostBuilder<TReceivePackage> ConfigureSuperSocket<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<ServerOptions> configurator)
            where TReceivePackage : class
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            ) as SuperSocketHostBuilder<TReceivePackage>;
        }

        public static IHostBuilder ConfigureSocketOptionsSetter(IHostBuilder hostBuilder, Func<Socket> socketOptionsSetter)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<Socket>>(socketOptionsSetter);
                }
            );
        }

        public static IServer BuildAsServer(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return host.AsServer();
        }

        public static IServer AsServer(this IHost host)
        {
            return host.Services.GetService<IEnumerable<IHostedService>>().OfType<IServer>().FirstOrDefault();
        }
    }
}
