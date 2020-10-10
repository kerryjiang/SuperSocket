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
        public static ISuperSocketHostBuilder UseUdp(this ISuperSocketHostBuilder hostBuilder)
        {
            return hostBuilder.UseUdp<IPAddressUdpSessionIdentifierProvider>();
        }

        public static ISuperSocketHostBuilder UseUdp<TUdpSessionIdentifierProvider>(this ISuperSocketHostBuilder hostBuilder)
            where TUdpSessionIdentifierProvider : class, IUdpSessionIdentifierProvider
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IChannelCreatorFactory, UdpChannelCreatorFactory>();
                services.AddSingleton<IUdpSessionIdentifierProvider, TUdpSessionIdentifierProvider>();
            }) as ISuperSocketHostBuilder;
        }
    }
}