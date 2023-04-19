using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Kestrel.Internal;

namespace SuperSocket.Kestrel;

public static class HostBuilderExtensions
{
    public static ISuperSocketHostBuilder UseKestrelChannelCreatorFactory(this ISuperSocketHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, server) =>
        {
            server.AddSingleton<KestrelChannelCreator>();
            server.AddSingleton<IKestrelChannelCreator>(s => s.GetRequiredService<KestrelChannelCreator>());
        });

        return hostBuilder.UseChannelCreatorFactory<KestrelChannelCreatorFactory>();
    }
}