using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;


namespace SuperSocket
{
    public static class HostBuilderExtensions
    {
        public static ISuperSocketHostBuilder AsSuperSocketBuilder(this IHostBuilder hostBuilder)
        {
            return hostBuilder as ISuperSocketHostBuilder;
        }

        public static ISuperSocketHostBuilder UseMiddleware<TMiddleware>(this ISuperSocketHostBuilder builder)
            where TMiddleware : class, IMiddleware
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
            }).AsSuperSocketBuilder();
        }

        public static ISuperSocketHostBuilder UseMiddleware<TMiddleware>(this ISuperSocketHostBuilder builder, Func<IServiceProvider, TMiddleware> implementationFactory)
            where TMiddleware : class, IMiddleware
        {
            return builder.ConfigureServices((ctx, services) => 
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>(implementationFactory));
            }).AsSuperSocketBuilder();
        }
    }
}
