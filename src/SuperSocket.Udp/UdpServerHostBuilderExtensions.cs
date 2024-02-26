using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Udp;


namespace SuperSocket
{
    public static class UdpServerHostBuilderExtensions
    {
        public static ISuperSocketHostBuilder UseUdp<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return (hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IConnectionListenerFactory, UdpConnectionListenerFactory>();
                services.AddSingleton<IConnectionFactoryBuilder, UdpConnectionFactoryBuilder<TReceivePackage>>();
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
    }
}