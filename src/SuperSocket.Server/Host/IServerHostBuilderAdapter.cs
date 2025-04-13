using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SuperSocket.Server.Host
{
    /// <summary>
    /// Defines methods to adapt and configure server host builders.
    /// </summary>
    public interface IServerHostBuilderAdapter
    {
        /// <summary>
        /// Configures the server with the specified host builder context and services.
        /// </summary>
        /// <param name="context">The context of the host builder.</param>
        /// <param name="hostServices">The collection of services for the host.</param>
        void ConfigureServer(HostBuilderContext context, IServiceCollection hostServices);

        /// <summary>
        /// Configures the service provider for the host.
        /// </summary>
        /// <param name="hostServiceProvider">The service provider for the host.</param>
        void ConfigureServiceProvider(IServiceProvider hostServiceProvider);
    }
}