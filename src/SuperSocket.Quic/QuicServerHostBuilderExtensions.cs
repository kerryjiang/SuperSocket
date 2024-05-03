using System;
using System.Net.Quic;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Quic;
#pragma warning disable CA2252

namespace SuperSocket.Server
{
    public static class QuicServerHostBuilderExtensions
    {
        public static ISuperSocketHostBuilder UseQuic(this ISuperSocketHostBuilder hostBuilder)
        {
            if (!QuicListener.IsSupported)
                throw new PlatformNotSupportedException("System.Net.Quic is not supported on this platform.");
            
            return hostBuilder.ConfigureServices((_, services) =>
            {
                services.AddSingleton<IConnectionStreamInitializersFactory, QuicConnectionStreamInitializersFactory>();
                services.AddSingleton<IConnectionListenerFactory, QuicConnectionListenerFactory>();
                services.AddSingleton<IConnectionFactoryBuilder, QuicConnectionFactoryBuilder>();
            }) as ISuperSocketHostBuilder;
        }

        public static ISuperSocketHostBuilder<TReceivePackage> UseQuic<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return (hostBuilder as ISuperSocketHostBuilder).UseQuic() as ISuperSocketHostBuilder<TReceivePackage>;
        }
    }
}
