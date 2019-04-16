using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;


namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {

        private const string _middlewareTypesTag = "MiddlewareTypes";

        public static IHostBuilder UseMiddleware<TMiddleware>(this IHostBuilder builder)
            where TMiddleware : IMiddleware
        {
            var types = builder.Properties[_middlewareTypesTag] as List<Type>;

            if (types == null)
                builder.Properties[_middlewareTypesTag] = types = new List<Type>();

            types.Add(typeof(TMiddleware));

            return builder;
        }
    }
}
