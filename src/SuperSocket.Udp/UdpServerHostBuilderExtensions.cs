using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Udp
{
    public static class UdpServerHostBuilderExtensions
    {
        public static ISuperSocketHostBuilder<TReceivePackage> UseUdp<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IChannelCreatorFactory, UdpChannelCreatorFactory>();
                services.AddSingleton<IUdpSessionIdentifierProvider, IPAddressUdpSessionIdentifierProvider>();
            }) as ISuperSocketHostBuilder<TReceivePackage>;
        }
    }
}