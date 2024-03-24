namespace SuperSocket.Kestrel;

using SuperSocket.Connection;
using Microsoft.Extensions.Hosting;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public static class KestrelPipeConnectionExtensions
{
    public static ISuperSocketHostBuilder UseKestrelPipeConnection(this ISuperSocketHostBuilder hostBuilder, SocketConnectionFactoryOptions options = null)
    {
        hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton(serviceProvider => new SocketConnectionContextFactory(options ?? new SocketConnectionFactoryOptions(), serviceProvider.GetService<ILoggerFactory>().CreateLogger<SocketConnectionContextFactory>()));
                services.AddSingleton<IConnectionFactoryBuilder, KestrelPipeConnectionFactoryBuilder>();
            });

        return hostBuilder;
    }
}
