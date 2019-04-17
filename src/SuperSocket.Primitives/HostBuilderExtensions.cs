using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket;


namespace SuperSocket
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseMiddleware<TMiddleware>(this IHostBuilder builder)
            where TMiddleware : class, IMiddleware
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
            });
        }
    }
}
