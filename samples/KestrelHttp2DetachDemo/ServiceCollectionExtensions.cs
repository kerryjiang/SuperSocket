using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    private const string SocketConnectionFactoryTypeName =
        "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory";

    public static IServiceCollection AddSocketConnectionFactory(this IServiceCollection services)
    {
        var factoryType = FindSocketConnectionFactory();
        return services.AddSingleton(typeof(IConnectionFactory), factoryType);
    }

    private static Type FindSocketConnectionFactory()
    {
        var assembly = typeof(SocketTransportOptions).Assembly;
        var connectionFactoryType = assembly.GetType(SocketConnectionFactoryTypeName);
        return connectionFactoryType ?? throw new NotSupportedException(SocketConnectionFactoryTypeName);
    }
}
