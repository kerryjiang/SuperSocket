using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions.Host
{
    /// <summary>
    /// Represents a builder for configuring and creating a SuperSocket host.
    /// </summary>
    public interface ISuperSocketHostBuilder : IHostBuilder, IMinimalApiHostBuilder
    {
        /// <summary>
        /// Configures additional services for the host.
        /// </summary>
        /// <param name="configureDelegate">The delegate to configure services.</param>
        ISuperSocketHostBuilder ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);
    }

    /// <summary>
    /// Represents a builder for configuring and creating a SuperSocket host with specific package type.
    /// </summary>
    /// <typeparam name="TReceivePackage">The type of packages that the server will receive.</typeparam>
    public interface ISuperSocketHostBuilder<TReceivePackage> : ISuperSocketHostBuilder
    {
        /// <summary>
        /// Configures the server options by reading from the provided configuration.
        /// </summary>
        /// <param name="serverOptionsReader">A function that reads server options from configuration.</param>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> ConfigureServerOptions(Func<HostBuilderContext, IConfiguration, IConfiguration> serverOptionsReader);

        /// <summary>
        /// Configures services for the host.
        /// </summary>
        /// <param name="configureDelegate">The delegate to configure services.</param>
        /// <returns>The host builder instance for chaining.</returns>
        new ISuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);

        /// <summary>
        /// Configures additional services for the host.
        /// </summary>
        /// <param name="configureDelegate">The delegate to configure services.</param>
        /// <returns>The host builder instance for chaining.</returns>
        new ISuperSocketHostBuilder<TReceivePackage> ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);

        /// <summary>
        /// Adds a middleware to the request processing pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The type of middleware to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UseMiddleware<TMiddleware>()
            where TMiddleware : class, IMiddleware;

        /// <summary>
        /// Sets the pipeline filter to use for processing incoming data.
        /// </summary>
        /// <typeparam name="TPipelineFilter">The type of the pipeline filter to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : class, IPipelineFilter<TReceivePackage>;

        /// <summary>
        /// Sets the pipeline filter factory to use for creating pipeline filters.
        /// </summary>
        /// <typeparam name="TPipelineFilterFactory">The type of the pipeline filter factory to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>;

        /// <summary>
        /// Registers a hosted service to be started and stopped with the server.
        /// </summary>
        /// <typeparam name="THostedService">The type of hosted service to register.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
            where THostedService : class, IHostedService;

        /// <summary>
        /// Sets the package decoder to use for decoding received data into packages.
        /// </summary>
        /// <typeparam name="TPackageDecoder">The type of package decoder to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TPackageDecoder>()
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>;

        /// <summary>
        /// Sets the package encoder to use for encoding packages into data to be sent.
        /// </summary>
        /// <typeparam name="TPackageEncoder">The type of package encoder to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePackageEncoder<TPackageEncoder>()
            where TPackageEncoder : class, IPackageEncoder<TReceivePackage>;
        
        /// <summary>
        /// Sets the package handling scheduler to use for scheduling package handling.
        /// </summary>
        /// <typeparam name="TPackageHandlingScheduler">The type of package handling scheduler to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingScheduler<TPackageHandlingScheduler>()
            where TPackageHandlingScheduler : class, IPackageHandlingScheduler<TReceivePackage>;

        /// <summary>
        /// Sets the session factory to use for creating new sessions.
        /// </summary>
        /// <typeparam name="TSessionFactory">The type of session factory to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UseSessionFactory<TSessionFactory>()
            where TSessionFactory : class, ISessionFactory;

        /// <summary>
        /// Sets the session type to use for handling client connections.
        /// </summary>
        /// <typeparam name="TSession">The type of session to use.</typeparam>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UseSession<TSession>()
            where TSession : IAppSession;

        /// <summary>
        /// Enables the package handling context accessor which allows access to the current package handling context.
        /// </summary>
        /// <returns>The host builder instance for chaining.</returns>
        ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingContextAccessor();
    }
}