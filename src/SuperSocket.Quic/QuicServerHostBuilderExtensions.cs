using System;
using System.Net.Quic;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Quic;

#pragma warning disable CA2252

namespace SuperSocket.Server;

public static class QuicServerHostBuilderExtensions
{
    public static ISuperSocketHostBuilder UseQuic(this ISuperSocketHostBuilder hostBuilder)
    {
        return hostBuilder.UseQuic(o => { });
    }

    public static ISuperSocketHostBuilder<TReceivePackage> UseQuic<TReceivePackage>(
        this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
    {
        return (hostBuilder as ISuperSocketHostBuilder).UseQuic(o => { }) as
            ISuperSocketHostBuilder<TReceivePackage>;
    }

    public static ISuperSocketHostBuilder<TReceivePackage> UseQuic<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder, Action<QuicTransportOptions> globalConfigure)
    {
        return (hostBuilder as ISuperSocketHostBuilder).UseQuic(globalConfigure) as
            ISuperSocketHostBuilder<TReceivePackage>;
    }

    public static ISuperSocketHostBuilder UseQuic(this ISuperSocketHostBuilder hostBuilder, Action<QuicTransportOptions> globalConfigure)
    {
        if (!QuicListener.IsSupported)
            throw new PlatformNotSupportedException("System.Net.Quic is not supported on this platform.");

        return hostBuilder.ConfigureServices((_, services) =>
        {
            services.Configure(globalConfigure);
            services.AddSingleton<IConnectionListenerFactory, QuicConnectionListenerFactory>();
            services.AddSingleton<IConnectionFactoryBuilder, QuicConnectionFactoryBuilder>();
        }) as ISuperSocketHostBuilder;
    }
}