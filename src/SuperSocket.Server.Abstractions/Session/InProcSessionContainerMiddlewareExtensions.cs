using System;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    public static class InProcSessionContainerMiddlewareExtensions
    {
        public static ISuperSocketHostBuilder UseInProcSessionContainer(this ISuperSocketHostBuilder builder)
        {
            return builder
                .UseMiddleware<InProcSessionContainerMiddleware>(s => s.GetRequiredService<InProcSessionContainerMiddleware>())
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<InProcSessionContainerMiddleware>();
                    services.AddSingleton<ISessionContainer>((s) => s.GetRequiredService<InProcSessionContainerMiddleware>());
                    services.AddSingleton<IAsyncSessionContainer>((s) => s.GetRequiredService<ISessionContainer>().ToAsyncSessionContainer());
                }) as ISuperSocketHostBuilder;
        }
    }
}
