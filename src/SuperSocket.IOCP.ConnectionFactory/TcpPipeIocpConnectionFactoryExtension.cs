using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.IOCP.ConnectionFactory;

public static class TcpPipeIocpConnectionFactoryExtension
{
    public static ISuperSocketHostBuilder UsePipeIocpConnection(this ISuperSocketHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((ctx, services) =>
        {
            services.AddSingleton<IConnectionFactoryBuilder, TcpPipeIocpConnectionFactoryBuilder>();
        });

        return hostBuilder;
    }
}