using System;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket.Tests;

public static class ServiceCollectionExtensions
{
    private const string SocketConnectionFactoryTypeName =
        "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory";

    private static Type FindSocketConnectionFactory()
    {
        var assembly = typeof(SocketTransportOptions).Assembly;
        var connectionFactoryType = assembly.GetType(SocketConnectionFactoryTypeName);
        return connectionFactoryType ?? throw new NotSupportedException(SocketConnectionFactoryTypeName);
    }

    public static IServiceCollection AddSocketConnectionFactory(this IServiceCollection services)
    {
        var factoryType = FindSocketConnectionFactory();
        return services.AddSingleton(typeof(IConnectionFactory), factoryType);
    }
}