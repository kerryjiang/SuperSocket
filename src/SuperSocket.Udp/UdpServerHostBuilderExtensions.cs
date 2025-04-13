using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Udp;

namespace SuperSocket.Server
{
    /// <summary>
    /// Provides extension methods for configuring a UDP server host builder.
    /// </summary>
    public static class UdpServerHostBuilderExtensions
    {
        /// <summary>
        /// Configures the host builder to use UDP.
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder UseUdp(this ISuperSocketHostBuilder hostBuilder)
        {
            return (hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IConnectionListenerFactory, UdpConnectionListenerFactory>();
                services.AddSingleton<IConnectionFactoryBuilder, UdpConnectionFactoryBuilder>();
            }) as ISuperSocketHostBuilder)
            .ConfigureSupplementServices((context, services) =>
            {
                if (!services.Any(s => s.ServiceType == typeof(IUdpSessionIdentifierProvider)))
                {
                    services.AddSingleton<IUdpSessionIdentifierProvider, IPAddressUdpSessionIdentifierProvider>();
                }

                if (!services.Any(s => s.ServiceType == typeof(IAsyncSessionContainer)))
                {
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, InProcSessionContainerMiddleware>(s => s.GetRequiredService<InProcSessionContainerMiddleware>()));
                    services.AddSingleton<InProcSessionContainerMiddleware>();
                    services.AddSingleton<ISessionContainer>((s) => s.GetRequiredService<InProcSessionContainerMiddleware>());
                    services.AddSingleton<IAsyncSessionContainer>((s) => s.GetRequiredService<ISessionContainer>().ToAsyncSessionContainer());
                }
            });
        }

        /// <summary>
        /// Configures the host builder to use UDP with a specific package type.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package to receive.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UseUdp<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return (hostBuilder as ISuperSocketHostBuilder).UseUdp() as ISuperSocketHostBuilder<TReceivePackage>;
        }
    }
}