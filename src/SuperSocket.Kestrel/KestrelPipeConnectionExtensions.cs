namespace SuperSocket.Kestrel;

using SuperSocket.Connection;
using Microsoft.Extensions.Hosting;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides extension methods for configuring SuperSocket to use Kestrel's pipe-based connection factory.
/// </summary>
public static class KestrelPipeConnectionExtensions
{
    /// <summary>
    /// Configures the SuperSocket host to use Kestrel's pipe-based connection factory.
    /// </summary>
    /// <param name="hostBuilder">The SuperSocket host builder.</param>
    /// <param name="options">The options for the socket connection factory. If <c>null</c>, default options are used.</param>
    /// <returns>The updated SuperSocket host builder.</returns>
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
