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
        public static SuperSocketHostBuilder<TReceivePackage> AsSuperSocketBuilder<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return hostBuilder as SuperSocketHostBuilder<TReceivePackage>;
        }

        public static SuperSocketHostBuilder<TReceivePackage> AsSuperSocketBuilder<TReceivePackage>(this IHostBuilder hostBuilder)
        {
            return hostBuilder as SuperSocketHostBuilder<TReceivePackage>;
        }

        public static SuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TReceivePackage>(this ISuperSocketHostBuilder hostBuilder, IPackageDecoder<TReceivePackage> packageDecoder)
        {
            return hostBuilder.AsSuperSocketBuilder<TReceivePackage>().UsePackageDecoder(packageDecoder);
        }

        public static SuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TReceivePackage, TPackageDecoder>(this ISuperSocketHostBuilder hostBuilder)
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>
        {
            return hostBuilder.AsSuperSocketBuilder<TReceivePackage>().UsePackageDecoder<TPackageDecoder>();
        }

        public static SuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TReceivePackage, TPipelineFilter>(this ISuperSocketHostBuilder hostBuilder)
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return hostBuilder.AsSuperSocketBuilder<TReceivePackage>().UsePipelineFilter<TPipelineFilter>();
        }

        public static SuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TReceivePackage, TPipelineFilterFactory>(this ISuperSocketHostBuilder hostBuilder)
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
        {
            return hostBuilder.AsSuperSocketBuilder<TReceivePackage>().UsePipelineFilterFactory<TPipelineFilterFactory>();
        }

        public static SuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
        {
            var builder = hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<object, IPipelineFilter<TReceivePackage>>>(filterFactory);
                }
            ) as SuperSocketHostBuilder<TReceivePackage>;

            return builder.UsePipelineFilterFactory<DelegatePipelineFilterFactory<TReceivePackage>>();
        }

        public static SuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TReceivePackage>(this ISuperSocketHostBuilder hostBuilder, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
        {
            return hostBuilder.AsSuperSocketBuilder<TReceivePackage>().UsePipelineFilterFactory(filterFactory);
        }


        public static SuperSocketHostBuilder<TReceivePackage> UseHostedService<TReceivePackage, THostedService>(this ISuperSocketHostBuilder hostBuilder)
            where THostedService : SuperSocketService<TReceivePackage>
        {
            return hostBuilder.AsSuperSocketBuilder<TReceivePackage>().UseHostedService<THostedService>();
        }

        public static ISuperSocketHostBuilder<TReceivePackage> UseSession<TReceivePackage, TSession>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
            where TSession : AppSession, new()
        {
            return hostBuilder.UseSession<TSession>() as SuperSocketHostBuilder<TReceivePackage>;
        }

  
        public static ISuperSocketHostBuilder UseSession<TSession>(this ISuperSocketHostBuilder hostBuilder)
            where TSession : AppSession, new()
        {
            return hostBuilder.UseSessionFactory<GenericSessionFactory<TSession>>();
        }

        public static ISuperSocketHostBuilder UseSessionFactory<TSessionFactory>(this ISuperSocketHostBuilder hostBuilder)
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

        public static ISuperSocketHostBuilder UseClearIdleSession(this ISuperSocketHostBuilder hostBuilder)
        {
            return hostBuilder.UseMiddleware<ClearIdleSessionMiddleware>();
        }

        public static SuperSocketHostBuilder<TReceivePackage> UseClearIdleSession<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return hostBuilder.UseMiddleware<ClearIdleSessionMiddleware>().AsSuperSocketBuilder<TReceivePackage>();
        }

        [Obsolete("Use the method UsePackageHandler instead.")]
        public static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageHandler<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, ValueTask> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
            where TReceivePackage : class
        {
            return hostBuilder.UsePackageHandler(packageHandler, errorHandler: errorHandler);
        }

        public static SuperSocketHostBuilder<TReceivePackage> UsePackageHandler<TReceivePackage>(this ISuperSocketHostBuilder hostBuilder, Func<IAppSession, TReceivePackage, ValueTask> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
            where TReceivePackage : class
        {
            return hostBuilder.AsSuperSocketBuilder<TReceivePackage>().UsePackageHandler(packageHandler, errorHandler: errorHandler);
        }

        [Obsolete("Use the method UsePackageHandler instead.")]
        public static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageHandler<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, TReceivePackage, ValueTask> packageHandler)
            where TReceivePackage : class
        {
            return hostBuilder.UsePackageHandler(packageHandler);
        }

        [Obsolete("Use the method UsePackageDecoder instead.")]
        public static SuperSocketHostBuilder<TReceivePackage> ConfigurePackageDecoder<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, IPackageDecoder<TReceivePackage> packageDecoder)
            where TReceivePackage : class
        {
            return hostBuilder.UsePackageDecoder(packageDecoder);
        }

        
        [Obsolete("Use the method UseSessionHandler instead.")]
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

        public static SuperSocketHostBuilder<TReceivePackage> UseSessionHandler<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<IAppSession, ValueTask> onConnected = null, Func<IAppSession, ValueTask> onClosed = null)
        {
            return (hostBuilder as ISuperSocketHostBuilder)
                .UseSessionHandler(onConnected, onClosed)
                .AsSuperSocketBuilder<TReceivePackage>();
        }

        public static ISuperSocketHostBuilder UseSessionHandler(this ISuperSocketHostBuilder hostBuilder, Func<IAppSession, ValueTask> onConnected = null, Func<IAppSession, ValueTask> onClosed = null)
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
            ) as ISuperSocketHostBuilder;
        }

        public static SuperSocketHostBuilder<TReceivePackage> ConfigureSuperSocket<TReceivePackage>(this SuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<ServerOptions> configurator)
        {
            return (hostBuilder as ISuperSocketHostBuilder)
                .ConfigureSuperSocket(configurator)
                .AsSuperSocketBuilder<TReceivePackage>();
        }

        public static SuperSocketHostBuilder<TReceivePackage> ConfigureSuperSocket<TReceivePackage>(this ISuperSocketHostBuilder hostBuilder, Action<ServerOptions> configurator)
        {
            return hostBuilder
                .ConfigureSuperSocket(configurator)
                .AsSuperSocketBuilder<TReceivePackage>();
        }

        public static ISuperSocketHostBuilder ConfigureSuperSocket(this ISuperSocketHostBuilder hostBuilder, Action<ServerOptions> configurator)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.Configure<ServerOptions>(configurator);
                }
            ) as ISuperSocketHostBuilder;
        }

        public static SuperSocketHostBuilder<TReceivePackage> ConfigureSocketOptionsSetter<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Func<Socket> socketOptionsSetter)
            where TReceivePackage : class
        {
            return (hostBuilder as ISuperSocketHostBuilder).ConfigureSocketOptionsSetter(socketOptionsSetter).AsSuperSocketBuilder<TReceivePackage>();
        }

        public static ISuperSocketHostBuilder ConfigureSocketOptionsSetter(this ISuperSocketHostBuilder hostBuilder, Func<Socket> socketOptionsSetter)
        {
            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<Socket>>(socketOptionsSetter);
                }
            ) as ISuperSocketHostBuilder;
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
