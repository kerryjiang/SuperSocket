using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.SessionContainer;
using SuperSocket.Udp;


namespace SuperSocket
{
    public static class UdpServerHostBuilderExtensions
    {
        public static ISuperSocketHostBuilder<TReceivePackage> UseUdp<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return (hostBuilder as ISuperSocketHostBuilder).UseUdp() as ISuperSocketHostBuilder<TReceivePackage>;
        }

        public static ISuperSocketHostBuilder UseUdp(this ISuperSocketHostBuilder hostBuilder)
        {
            return (hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IChannelCreatorFactory, UdpChannelCreatorFactory>();                
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