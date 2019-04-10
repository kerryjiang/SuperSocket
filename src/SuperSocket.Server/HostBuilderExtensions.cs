using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.Server;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSuperSocket(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSuperSocket(options => {});
        }

        public static IHostBuilder UseSuperSocket(this IHostBuilder hostBuilder, Action<ServerOptions> configureDelegate)
        {
            hostBuilder.ConfigureServices(
                (hostCtx, container) =>
                {
                    container.Add(new ServiceDescriptor(typeof(IHostedService), typeof(SuperSocketService), ServiceLifetime.Singleton));
                    container.Configure(configureDelegate);
                });

            return hostBuilder;
        }
    }
}
