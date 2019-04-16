using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSuperSocket<TReceivePackage, TPipelineFilter>(this IHostBuilder hostBuilder)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return hostBuilder.UseSuperSocketWithFilterFactory<TReceivePackage, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
        }

        public static IHostBuilder UseSuperSocket<TReceivePackage>(this IHostBuilder hostBuilder, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPipelineFilterFactory<TReceivePackage>>(new DelegatePipelineFilterFactory<TReceivePackage>(filterFactory));
                    services.AddSingleton<IHostedService, SuperSocketService<TReceivePackage>>();
                }
            );

            return hostBuilder;
        }


        public static IHostBuilder UseSuperSocketWithFilterFactory<TReceivePackage, TPipelineFilterFactory>(this IHostBuilder hostBuilder)
            where TReceivePackage : class
            where TPipelineFilterFactory : IPipelineFilterFactory<TReceivePackage>, new()
        {
            hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.TryAdd(new ServiceDescriptor(typeof(IListenerFactory), typeof(TcpSocketListenerFactory), ServiceLifetime.Singleton));
                    services.AddSingleton<IHostedService, SuperSocketService<TReceivePackage, TPipelineFilterFactory>>();
                }
            );

            return hostBuilder;
        }

        public static IHostBuilder ConfigurePackageHandler<TReceivePackage>(this IHostBuilder hostBuilder, Func<IAppSession, TReceivePackage, Task> packageHandler)
            where TReceivePackage : class
        {
            if (packageHandler == null)
            {
                return hostBuilder;
            }

            return hostBuilder.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession, TReceivePackage, Task>>(packageHandler);
                }
            );
        }

        public static IServer BuildAsServer(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return host.Services.GetService<IEnumerable<IHostedService>>().OfType<IServer>().FirstOrDefault();
        }
    }
}
