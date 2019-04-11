using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.Server;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSuperSocket<TReceivePackage>(this IHostBuilder hostBuilder)
            where TReceivePackage : class
        {
            return hostBuilder.UseSuperSocket<TReceivePackage>(options => {});
        }

        public static IHostBuilder UseSuperSocket<TReceivePackage>(this IHostBuilder hostBuilder, Action<ServerOptions> configureDelegate)
            where TReceivePackage : class
        {
            hostBuilder.ConfigureServices(
                (hostCtx, container) =>
                {
                    container.Add(new ServiceDescriptor(typeof(IHostedService), typeof(SuperSocketService<TReceivePackage>), ServiceLifetime.Singleton));
                    container.Configure(configureDelegate);
                });

            return hostBuilder;
        }
    }
}
