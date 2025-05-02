using System;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// Provides extension methods for configuring in-process session container middleware.
    /// </summary>
    public static class InProcSessionContainerMiddlewareExtensions
    {
        /// <summary>
        /// Configures the host builder to use the in-process session container middleware.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the received package.</typeparam>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <returns>The same instance of the <see cref="ISuperSocketHostBuilder{TReceivePackage}"/> for chaining.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UseInProcSessionContainer<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> builder)
        {
            return builder
                .UseMiddleware<InProcSessionContainerMiddleware>(s => s.GetRequiredService<InProcSessionContainerMiddleware>())
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<InProcSessionContainerMiddleware>();
                    services.AddSingleton<ISessionContainer>((s) => s.GetRequiredService<InProcSessionContainerMiddleware>());
                    services.AddSingleton<IAsyncSessionContainer>((s) => s.GetRequiredService<ISessionContainer>().ToAsyncSessionContainer());
                }) as ISuperSocketHostBuilder<TReceivePackage>;
        }
    }
}
