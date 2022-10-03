using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public interface ISuperSocketHostBuilder : IHostBuilder, IMinimalApiHostBuilder
    {
        ISuperSocketHostBuilder ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);
    }

    public interface ISuperSocketHostBuilder<TReceivePackage> : ISuperSocketHostBuilder
    {
        ISuperSocketHostBuilder<TReceivePackage> ConfigureServerOptions(Func<HostBuilderContext, IConfiguration, IConfiguration> serverOptionsReader);

        new ISuperSocketHostBuilder<TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);

        new ISuperSocketHostBuilder<TReceivePackage> ConfigureSupplementServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);

        ISuperSocketHostBuilder<TReceivePackage> UseMiddleware<TMiddleware>()
            where TMiddleware : class, IMiddleware;

        ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new();

        ISuperSocketHostBuilder<TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>;

        ISuperSocketHostBuilder<TReceivePackage> UseHostedService<THostedService>()
            where THostedService : class, IHostedService;

        ISuperSocketHostBuilder<TReceivePackage> UsePackageDecoder<TPackageDecoder>()
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>;

        ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingScheduler<TPackageHandlingScheduler>()
            where TPackageHandlingScheduler : class, IPackageHandlingScheduler<TReceivePackage>;

        ISuperSocketHostBuilder<TReceivePackage> UseSessionFactory<TSessionFactory>()
            where TSessionFactory : class, ISessionFactory;

        ISuperSocketHostBuilder<TReceivePackage> UseSession<TSession>()
            where TSession : IAppSession;

        ISuperSocketHostBuilder<TReceivePackage> UsePackageHandlingContextAccessor();
    }
}