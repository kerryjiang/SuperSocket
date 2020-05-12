using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.SessionContainer;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SuperSocket
{
    public static class InProcSessionContainerMiddlewareExtensions
    {
        public static ISuperSocketHostBuilder UseInProcSessionContainer(this ISuperSocketHostBuilder builder)
        {
            return builder.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<InProcSessionContainerMiddleware>();
                services.AddSingleton<ISessionContainer>((s) => s.GetRequiredService<InProcSessionContainerMiddleware>());
                services.AddSingleton<IAsyncSessionContainer>((s) => s.GetRequiredService<ISessionContainer>().ToAsyncSessionContainer());
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, InProcSessionContainerMiddleware>(s => s.GetRequiredService<InProcSessionContainerMiddleware>()));
            }) as ISuperSocketHostBuilder;
        }
    }
}
