using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SuperSocket.Server.Host
{
    public interface IServerHostBuilderAdapter
    {
        void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices);

        void ConfigureServiceProvider(IServiceProvider hostServiceProvider);
    }
}