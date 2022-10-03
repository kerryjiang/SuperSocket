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
using SuperSocket.Channel;

namespace SuperSocket
{
    public static class HostBuilderExtensions
    {
        public static ISuperSocketHostBuilder<TReceivePackage> AsSuperSocketHostBuilder<TReceivePackage>(this IHostBuilder hostBuilder)
        {
            if (hostBuilder is ISuperSocketHostBuilder<TReceivePackage> ssHostBuilder)
            {
                return ssHostBuilder;
            }

            return new SuperSocketHostBuilder<TReceivePackage>(hostBuilder);
        }

        public static ISuperSocketHostBuilder<TReceivePackage> AsSuperSocketHostBuilder<TReceivePackage, TPipelineFilter>(this IHostBuilder hostBuilder)
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            if (hostBuilder is ISuperSocketHostBuilder<TReceivePackage> ssHostBuilder)
            {
                return ssHostBuilder;
            }

            return (new SuperSocketHostBuilder<TReceivePackage>(hostBuilder))
                .UsePipelineFilter<TPipelineFilter>();
        }
 
        public static ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<object, IPipelineFilter<TReceivePackage>>>(filterFactory);
                }
            );

            return hostBuilder.UsePipelineFilterFactory<DelegatePipelineFilterFactory<TReceivePackage>>();
        }

        public static ISuperSocketHostBuilder<TReceivePackage> UseClearIdleSession<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return hostBuilder.UseMiddleware<ClearIdleSessionMiddleware>();
        }

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

        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureSuperSocket<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<ServerOptions> configurator)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            );
        }

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

        public static IServer BuildAsServer(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return host.AsServer();
        }

        public static IServer AsServer(this IHost host)
        {
            return host.Services.GetService<IEnumerable<IHostedService>>().OfType<IServer>().FirstOrDefault();
        }

        public static ISuperSocketHostBuilder<TReceivePackage> ConfigureErrorHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }

        // move to extensions
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

        public static MultipleServerHostBuilder AsMultipleServerHostBuilder(this IHostBuilder hostBuilder)
        {
            return new MultipleServerHostBuilder(hostBuilder);
        }

        public static IMinimalApiHostBuilder AsMinimalApiHostBuilder(this ISuperSocketHostBuilder hostBuilder)
        {
            return hostBuilder;
        }
    }
}
