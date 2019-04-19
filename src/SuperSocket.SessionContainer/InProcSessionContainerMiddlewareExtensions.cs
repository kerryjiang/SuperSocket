using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.SessionContainer;

namespace SuperSocket
{
    public static class InProcSessionContainerMiddlewareExtensions
    {
        public static IHostBuilder UseInProcSessionContainer(this IHostBuilder builder)
        {
            return builder.UseMiddleware<InProcSessionContainerMiddleware>();
        }
    }
}
