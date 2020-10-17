using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using System.Linq;
using SuperSocket.SessionContainer;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SuperSocket.Udp
{
    public static class UdpServerHostBuilderExtensions
    {
        public static ISuperSocketHostBuilder<TReceivePackage> UseUdp<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return hostBuilder.UseUdp() as ISuperSocketHostBuilder<TReceivePackage>;
        }

        public static ISuperSocketHostBuilder UseUdp(this ISuperSocketHostBuilder hostBuilder)
        {
            return (hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IChannelCreatorFactory, UdpChannelCreatorFactory>();
                services.AddSingleton<IUdpSessionIdentifierProvider, IPAddressUdpSessionIdentifierProvider>();
            }) as ISuperSocketHostBuilder).ConfigureSupplementServices((context, services) =>
            {
                if (services.Any(s => s.ServiceType == typeof(IAsyncSessionContainer)))
                    return;

                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, InProcSessionContainerMiddleware>(s => s.GetRequiredService<InProcSessionContainerMiddleware>()));
                services.AddSingleton<InProcSessionContainerMiddleware>();
                services.AddSingleton<ISessionContainer>((s) => s.GetRequiredService<InProcSessionContainerMiddleware>());
                services.AddSingleton<IAsyncSessionContainer>((s) => s.GetRequiredService<ISessionContainer>().ToAsyncSessionContainer());
            });
        }
    }
}